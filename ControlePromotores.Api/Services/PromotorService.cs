using Microsoft.EntityFrameworkCore;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.Models;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Services
{
    public class PromotorService
    {
        private readonly PromotoresContext _context;

        public PromotorService(PromotoresContext context)
        {
            _context = context;
        }

        public async Task<PromotorResponse> GetByIdAsync(int id)
        {
            var promotor = await _context.Promotores
                .Include(p => p.Empresa)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (promotor == null) return null;
            return MapearParaResponse(promotor);
        }

        public async Task<List<PromotorResponse>> GetAllAsync(int? empresaId = null)
        {
            var query = _context.Promotores
                .Include(p => p.Empresa)
                .Where(p => p.Ativo);

            if (empresaId.HasValue)
                query = query.Where(p => p.EmpresaId == empresaId.Value);

            var promotores = await query.ToListAsync();
            return promotores.Select(MapearParaResponse).ToList();
        }

        public async Task<PromotorResponse> CreateAsync(CriarPromotorRequest request)
        {
            // Verificar se empresa existe
            var empresa = await _context.Empresas.FindAsync(request.EmpresaId);
            if (empresa == null)
                throw new KeyNotFoundException("Empresa não encontrada");

            // Verificar CPF duplicado
            if (await _context.Promotores.AnyAsync(p => p.CPF == request.CPF))
                throw new InvalidOperationException("CPF já cadastrado");

            var promotor = new Promotor
            {
                Nome = request.Nome,
                CPF = request.CPF,
                Telefone = request.Telefone,
                Email = request.Email,
                Endereco = request.Endereco,
                Numero = request.Numero,
                Complemento = request.Complemento,
                Bairro = request.Bairro,
                Cidade = request.Cidade,
                Estado = request.Estado,
                CEP = request.CEP,
                EmpresaId = request.EmpresaId,
                Ativo = true,
                DataContratacao = DateTime.UtcNow
            };

            _context.Promotores.Add(promotor);
            await _context.SaveChangesAsync();

            promotor.Empresa = empresa;
            return MapearParaResponse(promotor);
        }

        public async Task<PromotorResponse> UpdateAsync(int id, AtualizarPromotorRequest request)
        {
            var promotor = await _context.Promotores.FindAsync(id);
            if (promotor == null) throw new KeyNotFoundException("Promotor não encontrado");

            // Verificar empresa
            var empresa = await _context.Empresas.FindAsync(request.EmpresaId);
            if (empresa == null)
                throw new KeyNotFoundException("Empresa não encontrada");

            // Verificar CPF duplicado
            if (promotor.CPF != request.CPF && await _context.Promotores.AnyAsync(p => p.CPF == request.CPF))
                throw new InvalidOperationException("CPF já cadastrado");

            promotor.Nome = request.Nome;
            promotor.CPF = request.CPF;
            promotor.Telefone = request.Telefone;
            promotor.Email = request.Email;
            promotor.Endereco = request.Endereco;
            promotor.Numero = request.Numero;
            promotor.Complemento = request.Complemento;
            promotor.Bairro = request.Bairro;
            promotor.Cidade = request.Cidade;
            promotor.Estado = request.Estado;
            promotor.CEP = request.CEP;
            promotor.EmpresaId = request.EmpresaId;

            _context.Promotores.Update(promotor);
            await _context.SaveChangesAsync();

            promotor.Empresa = empresa;
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
                Endereco = promotor.Endereco,
                Numero = promotor.Numero,
                Complemento = promotor.Complemento,
                Bairro = promotor.Bairro,
                Cidade = promotor.Cidade,
                Estado = promotor.Estado,
                CEP = promotor.CEP,
                DataContratacao = promotor.DataContratacao,
                Ativo = promotor.Ativo,
                EmpresaId = promotor.EmpresaId
            };
        }
    }
}
