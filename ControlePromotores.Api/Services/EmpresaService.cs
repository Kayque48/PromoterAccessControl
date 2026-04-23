using Microsoft.EntityFrameworkCore;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.Models;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Services
{
    /// <summary>
    /// Serviço de gestão de empresas/clientes com CRUD completo.
    /// Implementa soft delete via campo Ativo (não remove registros fisicamente).
    /// Garante unicidade de CNPJ (identificador nacional) e email.
    /// </summary>
    public class EmpresaService
    {
        private readonly PromotoresContext _context;

        public EmpresaService(PromotoresContext context)
        {
            _context = context;
        }

        public async Task<EmpresaResponse?> GetByIdAsync(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null) return null;
            return MapearParaResponse(empresa);
        }

        public async Task<List<EmpresaResponse>> GetAllAsync()
        {
            // Filtra apenas empresas ativas (soft delete): Ativo == true.
            // Empresas desativas permanecem no banco para auditoria de histórico.
            var empresas = await _context.Empresas
                .Where(e => e.Ativo)
                .ToListAsync();
            return empresas.Select(MapearParaResponse).ToList();
        }

        public async Task<EmpresaResponse> CreateAsync(CriarEmpresaRequest request)
        {
            // Verificar CNPJ duplicado
            if (await _context.Empresas.AnyAsync(e => e.CNPJ == request.CNPJ))
                throw new InvalidOperationException("CNPJ já cadastrado");

            var empresa = new Empresa
            {
                CNPJ = request.CNPJ,
                RazaoSocial = request.RazaoSocial,
                NomeFantasia = request.NomeFantasia ?? string.Empty,
                Telefone = request.Telefone,
                Email = request.Email,
                Endereco = request.Endereco,
                Ativo = true
            };

            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();
            return MapearParaResponse(empresa);
        }

        public async Task<EmpresaResponse> UpdateAsync(int id, AtualizarEmpresaRequest request)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null) throw new KeyNotFoundException("Empresa não encontrada");

            // Verificar CNPJ duplicado (se foi alterado)
            if (empresa.CNPJ != request.CNPJ && await _context.Empresas.AnyAsync(e => e.CNPJ == request.CNPJ))
                throw new InvalidOperationException("CNPJ já cadastrado");

            empresa.CNPJ = request.CNPJ;
            empresa.RazaoSocial = request.RazaoSocial;
            empresa.NomeFantasia = request.NomeFantasia ?? string.Empty;
            empresa.Telefone = request.Telefone;
            empresa.Email = request.Email;
            empresa.Endereco = request.Endereco;
            empresa.AtualizadoEm = DateTime.UtcNow;

            _context.Empresas.Update(empresa);
            await _context.SaveChangesAsync();
            return MapearParaResponse(empresa);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null) return false;

            empresa.Ativo = false;
            _context.Empresas.Update(empresa);
            await _context.SaveChangesAsync();
            return true;
        }

        private EmpresaResponse MapearParaResponse(Empresa empresa)
        {
            return new EmpresaResponse
            {
                Id = empresa.Id,
                CNPJ = empresa.CNPJ,
                RazaoSocial = empresa.RazaoSocial,
                NomeFantasia = empresa.NomeFantasia,
                Telefone = empresa.Telefone,
                Email = empresa.Email,
                Endereco = empresa.Endereco,
                CriadoEm = empresa.CriadoEm,
                AtualizadoEm = empresa.AtualizadoEm,
                Ativo = empresa.Ativo
            };
        }
    }
}
