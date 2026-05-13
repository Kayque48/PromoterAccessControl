using Microsoft.EntityFrameworkCore;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.Models;
using ControlePromotores.Api.DTOs;
using ControlePromotores.Api.Utils;

namespace ControlePromotores.Api.Services
{
    public class PromotorService
    {
        private const byte DiasPermitidosPadrao = 62; // Seg-Sex, conforme banco oficial.
        private readonly PromotoresContext _context;

        public PromotorService(PromotoresContext context)
        {
            _context = context;
        }

        public async Task<PromotorResponse?> GetByIdAsync(int id)
        {
            var promotor = await _context.Promotores
                .Include(p => p.EmpresaExclusiva)
                .Include(p => p.PromotorEmpresas)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promotor == null) return null;
            return MapearParaResponse(promotor);
        }

        public async Task<List<PromotorResponse>> GetAllAsync(int? empresaId = null)
        {
            var query = _context.Promotores
                .Include(p => p.EmpresaExclusiva)
                .Include(p => p.PromotorEmpresas)
                .Where(p => p.Ativo);

            if (empresaId.HasValue)
            {
                query = query.Where(p =>
                    p.EmpresaExclusivaId == empresaId.Value ||
                    p.PromotorEmpresas.Any(pe => pe.EmpresaId == empresaId.Value && pe.Ativo));
            }

            var promotores = await query.ToListAsync();
            return promotores.Select(MapearParaResponse).ToList();
        }

        public async Task<PromotorResponse> CreateAsync(CriarPromotorRequest request)
        {
            Empresa? empresa = null;
            if (request.EmpresaId > 0)
            {
                empresa = await _context.Empresas.FindAsync(request.EmpresaId);
                if (empresa == null)
                    throw new KeyNotFoundException("Empresa nao encontrada");
            }

            var cpfLimpo = NormalizarCpf(request.CPF);

            if (await _context.Promotores.AnyAsync(p => p.CPF == cpfLimpo))
                throw new InvalidOperationException("CPF ja cadastrado");

            // Tipo é determinado APENAS por EmpresaId, ignorando request.Tipo
            var tipo = request.EmpresaId > 0 ? "exclusivo" : "promotor";
            var diasPermitidos = ConverterDiasPermitidos(request.DiasPermitidos);

            var promotor = new Promotor
            {
                Nome = request.Nome,
                CPF = cpfLimpo,
                Telefone = request.Telefone ?? null!,
                Email = request.Email ?? null!,
                Tipo = tipo,
                EmpresaExclusivaId = tipo == "exclusivo" ? request.EmpresaId : null,
                Ativo = true
            };

            if (tipo == "promotor" && request.EmpresaId > 0)
            {
                promotor.PromotorEmpresas.Add(new PromotorEmpresa
                {
                    EmpresaId = request.EmpresaId.Value,
                    DiasPermitidos = diasPermitidos,
                    Ativo = true
                });
            }

            _context.Promotores.Add(promotor);
            await _context.SaveChangesAsync();

            if (empresa != null && promotor.EmpresaExclusivaId == empresa.Id)
                promotor.EmpresaExclusiva = empresa;

            return MapearParaResponse(promotor);
        }

        public async Task<PromotorResponse> UpdateAsync(int id, AtualizarPromotorRequest request)
        {
            var promotor = await _context.Promotores
                .Include(p => p.PromotorEmpresas)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promotor == null) throw new KeyNotFoundException("Promotor nao encontrado");

            Empresa? empresa = null;
            if (request.EmpresaId > 0)
            {
                empresa = await _context.Empresas.FindAsync(request.EmpresaId);
                if (empresa == null)
                    throw new KeyNotFoundException("Empresa nao encontrada");
            }

            var cpfLimpo = NormalizarCpf(request.CPF);

            if (promotor.CPF != cpfLimpo && await _context.Promotores.AnyAsync(p => p.CPF == cpfLimpo))
                throw new InvalidOperationException("CPF ja cadastrado");

            // Tipo é determinado APENAS por EmpresaId, ignorando request.Tipo
            var tipo = request.EmpresaId > 0 ? "exclusivo" : "promotor";
            var diasPermitidos = ConverterDiasPermitidos(request.DiasPermitidos);

            promotor.Nome = request.Nome;
            promotor.CPF = cpfLimpo;
            promotor.Telefone = request.Telefone ?? promotor.Telefone;
            promotor.Email = request.Email ?? promotor.Email;
            promotor.Tipo = tipo;
            promotor.AtualizadoEm = DateTime.UtcNow;

            if (tipo == "exclusivo")
            {
                promotor.EmpresaExclusivaId = request.EmpresaId;
                DesativarVinculos(promotor);
            }
            else
            {
                promotor.EmpresaExclusivaId = null;
                if (request.EmpresaId > 0)
                    GarantirVinculo(promotor, request.EmpresaId.Value, diasPermitidos);
            }

            _context.Promotores.Update(promotor);
            await _context.SaveChangesAsync();

            if (empresa != null && promotor.EmpresaExclusivaId == empresa.Id)
                promotor.EmpresaExclusiva = empresa;

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

        private static string NormalizarCpf(string cpf)
        {
            return cpf.Replace(".", "").Replace("-", "");
        }

        private static string NormalizarTipo(string? tipo)
        {
            return string.Equals(tipo?.Trim(), "exclusivo", StringComparison.OrdinalIgnoreCase)
                ? "exclusivo"
                : "promotor";
        }

        private static byte ConverterDiasPermitidos(string[]? dias)
        {
            if (dias == null || dias.Length == 0)
                return DiasPermitidosPadrao;

            var bitmask = DiasPermitidosHelper.DiaArrayParaBitmask(dias);
            if (bitmask <= 0 || bitmask > 127)
                throw new InvalidOperationException("Dias permitidos invalidos");

            return (byte)bitmask;
        }

        private static void GarantirVinculo(Promotor promotor, int empresaId, byte diasPermitidos)
        {
            var vinculo = promotor.PromotorEmpresas
                .FirstOrDefault(pe => pe.EmpresaId == empresaId);

            if (vinculo == null)
            {
                promotor.PromotorEmpresas.Add(new PromotorEmpresa
                {
                    EmpresaId = empresaId,
                    DiasPermitidos = diasPermitidos,
                    Ativo = true
                });
                return;
            }

            vinculo.DiasPermitidos = diasPermitidos;
            vinculo.Ativo = true;
        }

        private static void DesativarVinculos(Promotor promotor)
        {
            foreach (var vinculo in promotor.PromotorEmpresas.Where(pe => pe.Ativo))
                vinculo.Ativo = false;
        }

        private static PromotorResponse MapearParaResponse(Promotor promotor)
        {
            var vinculosAtivos = promotor.PromotorEmpresas
                .Where(pe => pe.Ativo)
                .OrderBy(pe => pe.Id)
                .ToList();

            var primeiroVinculo = vinculosAtivos.FirstOrDefault();
            var empresaIds = promotor.EmpresaExclusivaId.HasValue
                ? new[] { promotor.EmpresaExclusivaId.Value }
                : vinculosAtivos.Select(pe => pe.EmpresaId).ToArray();

            return new PromotorResponse
            {
                Id = promotor.Id,
                Nome = promotor.Nome,
                CPF = promotor.CPF,
                Telefone = promotor.Telefone,
                Email = promotor.Email,
                Tipo = promotor.Tipo,
                EmpresaId = promotor.EmpresaExclusivaId ?? primeiroVinculo?.EmpresaId,
                EmpresaIds = empresaIds,
                EmpresaExclusivaId = promotor.EmpresaExclusivaId,
                // DiasPermitidos nunca é null. Se sem vínculo, retorna default (Seg-Sex)
                DiasPermitidos = primeiroVinculo == null
                    ? DiasPermitidosHelper.BitmaskParaDiaArray(DiasPermitidosPadrao)
                    : DiasPermitidosHelper.BitmaskParaDiaArray(primeiroVinculo.DiasPermitidos),
                CriadoEm = promotor.CriadoEm,
                AtualizadoEm = promotor.AtualizadoEm,
                Ativo = promotor.Ativo
            };
        }
    }
}
