using Asp.Versioning;
using AutoMapper;
using DevIO.Api.Controllers;
using DevIO.Api.Extensions;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProdutosController : MainController
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;
        public ProdutosController(INotificador notificador, IProdutoRepository produtoRepository, IProdutoService produtoService, IMapper mapper, IUser user) : base(notificador, user)
        {
            _produtoRepository = produtoRepository;
            _produtoService = produtoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ProdutoViewModel>> ObterTodos()
        {
            return _mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoRepository.ObterProdutosFornecedores());
        }


        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId(Guid id)
        {
            var produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null)
            {
                return NotFound();
            }

            return produtoViewModel;
        }

        [HttpDelete]
        public async Task<ActionResult<ProdutoViewModel>> Excluir(Guid id)
        {
            var produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null)
            {
                return NotFound();
            }

            await _produtoService.Remover(id);

            return CustomResponse();
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar(ProdutoViewModel produtoViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imagemNome = Guid.NewGuid() + "_" + produtoViewModel.Imagem;

            if (!UploadArquivo(produtoViewModel.ImagemUpload, imagemNome))
            {
                return CustomResponse();
            }

            produtoViewModel.Imagem = imagemNome;
            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoViewModel));

            return CustomResponse(produtoViewModel);
        }

        [HttpPost("Adicionar")]
        public async Task<ActionResult<ProdutoViewModel>> AdicionarAlternativo(
            // Binder personalizado para envio de IFormFile e ViewModel dentro de um FormData compatível com .NET Core 3.1 ou superior (system.text.json)
            [ModelBinder(BinderType = typeof(ProdutoModelBinder))]
            ProdutoImagemViewModel produtoViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imgPrefixo = Guid.NewGuid() + "_";
            if (!await UploadArquivoAlternativo(produtoViewModel.ImagemUpload, imgPrefixo))
            {
                return CustomResponse(ModelState);
            }

            produtoViewModel.Imagem = imgPrefixo + produtoViewModel.ImagemUpload.FileName;
            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoViewModel));

            return CustomResponse(produtoViewModel);
        }

        private bool UploadArquivo(string arquivo, string imgNome)
        {
            if (string.IsNullOrEmpty(arquivo))
            {
                //Pode adicionar o erro na ModelState ou no notificador
                //ModelState.AddModelError(string.Empty, "Forneça uma imagem para este produto!");
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }

            var imageDataByteArray = Convert.FromBase64String(arquivo);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens", imgNome);
            if (System.IO.File.Exists(filePath))
            {
                NotificarErro("Já existe um arquivo com este nome!");
                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);

            return true;
        }


        private async Task<bool> UploadArquivoAlternativo(IFormFile arquivo, string imgPrefixo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imgPrefixo + arquivo.FileName);

            if (System.IO.File.Exists(path))
            {
                NotificarErro("Já existe um arquivo com este nome!");
                return false;
            }

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }

        private async Task<ProdutoViewModel> ObterProduto(Guid id)
        {
            return _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutoFornecedor(id));

        }
    }
}
