using AutoMapper;
using DevIO.Api.ViewModels;
using DevIO.Business.Models;

namespace DevIO.Api.Configuration
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig() 
        {
            // Mapeando a model fornecedor para view model
            // ReverseMap mapeia da view model para a model
            CreateMap<Fornecedor, FornecedorViewModel>().ReverseMap();
            CreateMap<Endereco, EnderecoViewModel>().ReverseMap();
            CreateMap<ProdutoViewModel, Produto>();

            // retorna o nome do fornecedor do produto, pois não é feito através do ReverseMap
            CreateMap<Produto, ProdutoViewModel>().ForMember(dest => dest.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome));
        }
    }
}
