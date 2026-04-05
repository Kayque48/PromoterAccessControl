using Microsoft.EntityFrameworkCore;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.Models;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Services
{
    public class RegistroAcessoService
    {
        private readonly PromotoresContext _context;

        public RegistroAcessoService(PromotoresContext context)
        {
            _context = context;
        }

        public async Task<RegistroAcessoResponse> RegistrarEntradaAsync(int promotorId)
        {
            var promotor = await _context.Promotores.FindAsync(promotorId);
            if (promotor == null)
                throw new KeyNotFoundException("Promotor não encontrado");

            // Verificar se já existe entrada sem saída
            var registroAberto = await _context.RegistrosAcesso
                .FirstOrDefaultAsync(r => r.PromotorId == promotorId && r.Saida == null);

            if (registroAberto != null)
                throw new InvalidOperationException("Promotor já possui registro de entrada aberto");

            var registro = new RegistroAcesso
            {
                PromotorId = promotorId,
                Entrada = DateTime.UtcNow,
                Saida = null
            };

            _context.RegistrosAcesso.Add(registro);
            await _context.SaveChangesAsync();

            return MapearParaResponse(registro, promotor.Nome);
        }

        public async Task<RegistroAcessoResponse> RegistrarSaidaAsync(int registroId)
        {
            var registro = await _context.RegistrosAcesso
                .Include(r => r.Promotor)
                .FirstOrDefaultAsync(r => r.Id == registroId);

            if (registro == null)
                throw new KeyNotFoundException("Registro não encontrado");

            if (registro.Saida != null)
                throw new InvalidOperationException("Este registro já possui saída registrada");

            registro.Saida = DateTime.UtcNow;
            registro.TempoPermanenciaMinutos = (int)Math.Floor((registro.Saida.Value - registro.Entrada).TotalMinutes);

            _context.RegistrosAcesso.Update(registro);
            await _context.SaveChangesAsync();

            return MapearParaResponse(registro, registro.Promotor.Nome);
        }

        public async Task<List<PromotorAtativoResponse>> GetPromotoresAtivosAsync()
        {
            var agora = DateTime.UtcNow;

            var registrosAbertos = await _context.RegistrosAcesso
                .Include(r => r.Promotor)
                .Where(r => r.Saida == null)
                .ToListAsync();

            return registrosAbertos.Select(r => new PromotorAtativoResponse
            {
                Id = r.PromotorId,
                Nome = r.Promotor.Nome,
                Entrada = r.Entrada,
                MinutosAtendimento = (int)Math.Floor((agora - r.Entrada).TotalMinutes)
            }).ToList();
        }

        public async Task<List<RegistroAcessoResponse>> GetRegistrosByPromotorAsync(int promotorId, DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            var query = _context.RegistrosAcesso
                .Include(r => r.Promotor)
                .Where(r => r.PromotorId == promotorId);

            if (dataInicio.HasValue)
                query = query.Where(r => r.Entrada >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(r => r.Entrada <= dataFim.Value);

            var registros = await query.ToListAsync();
            return registros.Select(r => MapearParaResponse(r, r.Promotor.Nome)).ToList();
        }

        public async Task<List<RegistroAcessoResponse>> GetRegistrosByEmpresaAsync(int empresaId, DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            var query = _context.RegistrosAcesso
                .Include(r => r.Promotor)
                .ThenInclude(p => p.Empresa)
                .Where(r => r.Promotor.EmpresaId == empresaId);

            if (dataInicio.HasValue)
                query = query.Where(r => r.Entrada >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(r => r.Entrada <= dataFim.Value);

            var registros = await query.ToListAsync();
            return registros.Select(r => MapearParaResponse(r, r.Promotor.Nome)).ToList();
        }

        public async Task<RegistroAcessoResponse> GetByIdAsync(int id)
        {
            var registro = await _context.RegistrosAcesso
                .Include(r => r.Promotor)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registro == null) return null;
            return MapearParaResponse(registro, registro.Promotor.Nome);
        }

        private RegistroAcessoResponse MapearParaResponse(RegistroAcesso registro, string promotorNome)
        {
            return new RegistroAcessoResponse
            {
                Id = registro.Id,
                Entrada = registro.Entrada,
                Saida = registro.Saida,
                TempoPermanenciaMinutos = registro.TempoPermanenciaMinutos,
                PromotorId = registro.PromotorId,
                PromotorNome = promotorNome
            };
        }
    }
}
