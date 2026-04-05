using Microsoft.EntityFrameworkCore;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.Models;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Services
{
    /// <summary>
    /// Serviço de gestão de promotores com suporte a dois modelos de alocação:
    /// 1. Promotor genérico: Trabalha para múltiplas empresas (relacionamento N:N via PromotorEmpresa).
    /// 2. Promotor exclusivo: Vinculado a uma única empresa (EmpresaExclusivaId).
    /// Garante unicidade de CPF e implementa soft delete para manutenção de histórico.
    /// </summary>
    public class PromotorService
    {
        private readonly PromotoresContext _context;

        public PromotorService(PromotoresContext context)
        {
            _context = context;
        }

        public async Task<PromotorResponse?> GetByIdAsync(int id)
        {
            var promotor = await _context.Promotores
                .Include(p => p.EmpresaExclusiva)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promotor == null) return null;
            return MapearParaResponse(promotor);
        }

        public async Task<List<PromotorResponse>> GetAllAsync(int? empresaId = null)
        {
            var query = _context.Promotores
                .Include(p => p.EmpresaExclusiva)
                .Where(p => p.Ativo);

            // Filtro opcional: Se empresaId informado, retorna apenas promotores exclusivos daquela empresa.
            // Caso base (null): Retorna todos os promotores genéricos + exclusivos (para backend/admin).
            if (empresaId.HasValue)
                query = query.Where(p => p.EmpresaExclusivaId == empresaId.Value);

            var promotores = await query.ToListAsync();
            return promotores.Select(MapearParaResponse).ToList();
        }

        public async Task<PromotorResponse> CreateAsync(CriarPromotorRequest request)
        {
            // Lógica de negócio: Tipo de promotor é determinado pela presença de EmpresaId no request.
            // - Se EmpresaId > 0: Cria promotor exclusivo (Tipo="exclusivo", EmpresaExclusivaId preenchido).
            // - Caso contrário: Cria promotor genérico (Tipo="promotor", sem empresa exclusiva).
            Empresa? empresaExclusiva = null;
            if (request.EmpresaId > 0)
            {
                empresaExclusiva = await _context.Empresas.FindAsync(request.EmpresaId);
                if (empresaExclusiva == null)
                    throw new KeyNotFoundException("Empresa não encontrada");
            }

            // Validação de data: CPF é identificador único nacional brasileiro.
            // Impede duplicação: Dois promotores não podem ter o mesmo CPF.
            if (await _context.Promotores.AnyAsync(p => p.CPF == request.CPF))
                throw new InvalidOperationException("CPF já cadastrado");

            var promotor = new Promotor
            {
                Nome = request.Nome,
                CPF = request.CPF,
                Telefone = request.Telefone ?? null!,
                Email = request.Email ?? null!,
                Tipo = request.EmpresaId > 0 ? "exclusivo" : "promotor",
                EmpresaExclusivaId = request.EmpresaId > 0 ? request.EmpresaId : null,
                Ativo = true
            };

            _context.Promotores.Add(promotor);
            await _context.SaveChangesAsync();

            if (empresaExclusiva != null)
                promotor.EmpresaExclusiva = empresaExclusiva;

            return MapearParaResponse(promotor);
        }

        public async Task<PromotorResponse> UpdateAsync(int id, AtualizarPromotorRequest request)
        {
            var promotor = await _context.Promotores.FindAsync(id);
            if (promotor == null) throw new KeyNotFoundException("Promotor não encontrado");

            // Se mudou para exclusivo, verificar empresa
            Empresa? empresaExclusiva = null;
            if (request.EmpresaId > 0)
            {
                empresaExclusiva = await _context.Empresas.FindAsync(request.EmpresaId);
                if (empresaExclusiva == null)
                    throw new KeyNotFoundException("Empresa não encontrada");
            }

            // Verificar CPF duplicado
            if (promotor.CPF != request.CPF && await _context.Promotores.AnyAsync(p => p.CPF == request.CPF))
                throw new InvalidOperationException("CPF já cadastrado");

            promotor.Nome = request.Nome;
            promotor.CPF = request.CPF;
            promotor.Telefone = request.Telefone ?? promotor.Telefone;
            promotor.Email = request.Email ?? promotor.Email;
            promotor.Tipo = request.EmpresaId > 0 ? "exclusivo" : "promotor";
            promotor.EmpresaExclusivaId = request.EmpresaId > 0 ? request.EmpresaId : null;
            promotor.AtualizadoEm = DateTime.UtcNow;

            _context.Promotores.Update(promotor);
            await _context.SaveChangesAsync();

            if (empresaExclusiva != null)
                promotor.EmpresaExclusiva = empresaExclusiva;

            return MapearParaResponse(promotor);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var promotor = await _context.Promotores.FindAsync(id);
            if (promotor == null) return false;

            promotor.Ativo = false;
            _context.Promotores.Update(promotor);
            await _context.SaveChangesAsync();
            return true;
        }

        private PromotorResponse MapearParaResponse(Promotor promotor)
        {
            return new PromotorResponse
            {
                Id = promotor.Id,
                Nome = promotor.Nome,
                CPF = promotor.CPF,
                Telefone = promotor.Telefone,
                Email = promotor.Email,
                Tipo = promotor.Tipo,
                EmpresaExclusivaId = promotor.EmpresaExclusivaId,
                CriadoEm = promotor.CriadoEm,
                AtualizadoEm = promotor.AtualizadoEm,
                Ativo = promotor.Ativo
            };
        }
    }
}
