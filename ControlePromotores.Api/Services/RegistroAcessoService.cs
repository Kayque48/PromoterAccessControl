using Microsoft.EntityFrameworkCore;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.Models;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Services
{
    /// <summary>
    /// Serviço que gerencia o ciclo de vida de registros de acesso (entrada/saída).
    /// Implementa validações de negócio: impede entradas duplicadas, calcula permanência ao registrar saída.
    /// Modelo: Cada visita = 2 registros (entrada + saída) com timestamp e duração armazenados.
    /// </summary>
    public class RegistroAcessoService
    {
        private readonly PromotoresContext _context;

        public RegistroAcessoService(PromotoresContext context)
        {
            _context = context;
        }

        public async Task<RegistroResponse> RegistrarEntradaAsync(int promotorId, int empresaId, int usuarioId, string? observacao = null)
        {
            var promotor = await _context.Promotores.FindAsync(promotorId);
            if (promotor == null)
                throw new KeyNotFoundException("Promotor não encontrado.");

            var empresa = await _context.Empresas.FindAsync(empresaId);
            if (empresa == null)
                throw new KeyNotFoundException("Empresa não encontrada.");

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            var hoje = DateTime.UtcNow.Date;

            // Validação de negócio: Impede entradas duplicadas (promotor não pode ter 2 entradas abertas no mesmo dia/empresa).
            // Consulta: Busca entrada do dia atual SEM saída correspondente (saída posterior).
            // Garante que cada promotor tem no máximo 1 visita ativa por empresa.
            var entradaAberta = await _context.Registros
                .Where(r => r.PromotorId == promotorId
                            && r.EmpresaId == empresaId
                            && r.Tipo == "entrada"
                            && r.DataHora >= hoje)
                .AnyAsync(r => !_context.Registros.Any(rs =>
                    rs.PromotorId == r.PromotorId &&
                    rs.EmpresaId == r.EmpresaId &&
                    rs.Tipo == "saida" &&
                    rs.DataHora > r.DataHora));

            if (entradaAberta)
                throw new InvalidOperationException("Já existe uma entrada em aberto para este promotor nesta empresa.");

            var registro = new Registro
            {
                PromotorId = promotorId,
                EmpresaId = empresaId,
                Tipo = "entrada",
                DataHora = DateTime.UtcNow,
                RegistradoPor = usuarioId,
                Observacao = observacao ?? null!
            };

            _context.Registros.Add(registro);
            await _context.SaveChangesAsync();

            return await MapearParaResponseAsync(registro.Id);
        }

        public async Task<RegistroResponse> RegistrarSaidaAsync(int promotorId, int empresaId, int usuarioId, string? observacao = null)
        {
            var promotor = await _context.Promotores.FindAsync(promotorId);
            if (promotor == null)
                throw new KeyNotFoundException("Promotor não encontrado.");

            var empresa = await _context.Empresas.FindAsync(empresaId);
            if (empresa == null)
                throw new KeyNotFoundException("Empresa não encontrada.");

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            var hoje = DateTime.UtcNow.Date;

            // Busca entrada aberta (sem saída posterior) no mesmo dia/empresa.
            var entradaAberta = await _context.Registros
                .Where(r => r.PromotorId == promotorId
                            && r.EmpresaId == empresaId
                            && r.Tipo == "entrada"
                            && r.DataHora >= hoje)
                .Where(r => !_context.Registros.Any(rs =>
                    rs.PromotorId == r.PromotorId &&
                    rs.EmpresaId == r.EmpresaId &&
                    rs.Tipo == "saida" &&
                    rs.DataHora > r.DataHora))
                .OrderByDescending(r => r.DataHora)
                .FirstOrDefaultAsync();

            if (entradaAberta == null)
                throw new InvalidOperationException("Não há entrada em aberto para registrar saída.");

            // Cálculo de permanência: Diferença em minutos entre saída e entrada.
            // Armazenado na tabela Registros para análise: duração média de visita, time-on-site, etc.
            var dataSaida = DateTime.UtcNow;
            var permanenciaMin = (int)Math.Floor((dataSaida - entradaAberta.DataHora).TotalMinutes);

            var registro = new Registro
            {
                PromotorId = promotorId,
                EmpresaId = empresaId,
                Tipo = "saida",
                DataHora = dataSaida,
                PermanenciaMin = permanenciaMin,
                RegistradoPor = usuarioId,
                Observacao = observacao ?? null!
            };

            _context.Registros.Add(registro);
            await _context.SaveChangesAsync();

            return await MapearParaResponseAsync(registro.Id);
        }

        public async Task<RegistroResponse> RegistrarSaidaPorRegistroIdAsync(int registroEntradaId, int usuarioId, string? observacao = null)
        {
            var entrada = await _context.Registros.FindAsync(registroEntradaId);
            if (entrada == null || entrada.Tipo != "entrada")
                throw new KeyNotFoundException("Registro de entrada não encontrado.");

            return await RegistrarSaidaAsync(entrada.PromotorId, entrada.EmpresaId, usuarioId, observacao);
        }

        public async Task<List<RegistroSessaoResponse>> GetAllSessaoAsync()
        {
            var registros = await _context.Registros
                .Include(r => r.Promotor)
                .Include(r => r.Empresa)
                .Where(r => r.Tipo == "entrada" || r.Tipo == "saida")
                .OrderBy(r => r.DataHora)
                .ToListAsync();

            var saidas = registros
                .Where(r => r.Tipo == "saida")
                .ToList();

            return registros
                .Where(r => r.Tipo == "entrada")
                .Select(entrada => new RegistroSessaoResponse
                {
                    Id = entrada.Id,
                    PromoterId = entrada.PromotorId,
                    CompanyId = entrada.EmpresaId,
                    PromotorNome = entrada.Promotor?.Nome ?? string.Empty,
                    EmpresaNome = entrada.Empresa?.RazaoSocial ?? string.Empty,
                    EntryTime = entrada.DataHora,
                    ExitTime = saidas
                        .Where(saida => saida.PromotorId == entrada.PromotorId
                                        && saida.EmpresaId == entrada.EmpresaId
                                        && saida.DataHora > entrada.DataHora)
                        .OrderBy(saida => saida.DataHora)
                        .Select(saida => (DateTime?)saida.DataHora)
                        .FirstOrDefault()
                })
                .ToList();
        }

        public async Task<List<PromotorAtivoResponse>> GetPromotoresAtivosAsync()
        {
            var agora = DateTime.UtcNow;
            var hoje = agora.Date;

            var entradasAbertas = await _context.Registros
                .Include(r => r.Promotor)
                .Include(r => r.Empresa)
                .Where(r => r.Tipo == "entrada" && r.DataHora >= hoje)
                .Where(r => !_context.Registros.Any(rs =>
                    rs.PromotorId == r.PromotorId &&
                    rs.EmpresaId == r.EmpresaId &&
                    rs.Tipo == "saida" &&
                    rs.DataHora > r.DataHora))
                .OrderByDescending(r => r.DataHora)
                .ToListAsync();

            return entradasAbertas.Select(r => new PromotorAtivoResponse
            {
                PromotorId = r.PromotorId,
                PromotorNome = r.Promotor.Nome,
                EmpresaId = r.EmpresaId,
                EmpresaNome = r.Empresa.RazaoSocial,
                EntradaEm = r.DataHora,
                MinutosEmAtendimento = (int)Math.Floor((agora - r.DataHora).TotalMinutes)
            }).ToList();
        }

        public async Task<List<RegistroResponse>> GetRegistrosByPromotorAsync(int promotorId, DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            var query = _context.Registros
                .Include(r => r.Promotor)
                .Include(r => r.Empresa)
                .Include(r => r.UsuarioRegistrador)
                .Where(r => r.PromotorId == promotorId)
                .AsQueryable();

            if (dataInicio.HasValue)
                query = query.Where(r => r.DataHora >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(r => r.DataHora <= dataFim.Value);

            var registros = await query
                .OrderByDescending(r => r.DataHora)
                .ToListAsync();

            return registros.Select(MapearParaResponse).ToList();
        }

        public async Task<List<RegistroResponse>> GetRegistrosByEmpresaAsync(int empresaId, DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            var query = _context.Registros
                .Include(r => r.Promotor)
                .Include(r => r.Empresa)
                .Include(r => r.UsuarioRegistrador)
                .Where(r => r.EmpresaId == empresaId)
                .AsQueryable();

            if (dataInicio.HasValue)
                query = query.Where(r => r.DataHora >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(r => r.DataHora <= dataFim.Value);

            var registros = await query
                .OrderByDescending(r => r.DataHora)
                .ToListAsync();

            return registros.Select(MapearParaResponse).ToList();
        }

        public async Task<RegistroResponse?> GetByIdAsync(int id)
        {
            var registro = await _context.Registros
                .Include(r => r.Promotor)
                .Include(r => r.Empresa)
                .Include(r => r.UsuarioRegistrador)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registro == null)
                return null;

            return MapearParaResponse(registro);
        }

        private async Task<RegistroResponse> MapearParaResponseAsync(int registroId)
        {
            var registro = await _context.Registros
                .Include(r => r.Promotor)
                .Include(r => r.Empresa)
                .Include(r => r.UsuarioRegistrador)
                .FirstAsync(r => r.Id == registroId);

            return MapearParaResponse(registro);
        }

        private RegistroResponse MapearParaResponse(Registro registro)
        {
            return new RegistroResponse
            {
                Id = registro.Id,
                PromotorId = registro.PromotorId,
                PromotorNome = registro.Promotor?.Nome ?? string.Empty,
                EmpresaId = registro.EmpresaId,
                EmpresaNome = registro.Empresa?.RazaoSocial ?? string.Empty,
                Tipo = registro.Tipo,
                DataHora = registro.DataHora,
                PermanenciaMin = registro.PermanenciaMin,
                RegistradoPor = registro.RegistradoPor,
                NomeUsuario = registro.UsuarioRegistrador?.Nome ?? string.Empty,
                Observacao = registro.Observacao
            };
        }
    }
}