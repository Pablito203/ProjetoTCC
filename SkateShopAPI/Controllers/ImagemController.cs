﻿using Microsoft.AspNetCore.Mvc;
using SkateShopAPI.EntityModels;
using SkateShopAPI.ModelsAPI;
using SkateShopAPI.Services;

namespace SkateShopAPI.Controllers {

    [ApiController]
    [Route("api/[Controller]")]
    public class ImagemController : ControllerBase {

        [HttpGet("{id}")]
        public RespostaAPI GetImagem(int id) {
            Repository repository = new();

            var lstImagens = repository.FilterQuery<Anexo>((p) => p.Produto == id).Select((p) => new ImagemRetorno {
                ImagemID = p.Anexo1,
                Nome = p.Nome,
                CaminhoImagem = p.CaminhoRelativo
            }).ToList();

            repository.Dispose();
            foreach (var Imagem in lstImagens) {
                if (Imagem.CaminhoImagem is not null)
                {
                    Imagem.CaminhoImagem = AnexoService.BuscarArquivoBase64(Imagem.CaminhoImagem);
                }
            }

            return new RespostaAPI(lstImagens);
        }

        [HttpPost]
        public RespostaAPI PostImagem(IFormCollection FormData) {
            if (!FormData.TryGetValue("ProdutoID", out var strProdutoID)) {
                return new RespostaAPI("Produto inválido");
            }

            if (!FormData.TryGetValue("Nome", out var strNome)) {
                return new RespostaAPI("Nome inválido");
            }

            if (!FormData.Files.Any()) {
                return new RespostaAPI("Arquivo inválido");
            }

            int ProdutoID = int.Parse(strProdutoID.First());
            if (ProdutoID <= 0) {
                return new RespostaAPI("Registro relacionado não encontrado");
            }

            try {
                string CaminhoRelativoDiretorio = AnexoService.CriarCaminhoRelativoDiretorioProduto(ProdutoID);

                AnexoService.OpcoesSalvarArquivo opcoes = new AnexoService.OpcoesSalvarArquivo {
                    NomeGuid = Guid.NewGuid().ToString(),
                    CaminhoRelativo = CaminhoRelativoDiretorio,
                    Arquivo = FormData.Files.First()
                };

                AnexoService.SalvarArquivo(opcoes);

                Anexo Anexo = new Anexo() {
                    Produto = ProdutoID,
                    Nome = strNome.First(),
                    CaminhoRelativo = Path.Combine(CaminhoRelativoDiretorio, opcoes.NomeGuid + ".jpg")
                };

                Repository Repository = new();
                Repository.Insert(Anexo);
                Repository.Dispose();

                return new RespostaAPI(new { sucesso = true });
            }
            catch {
                return new RespostaAPI("Registro relacionado não encontrado");
            }
        }

        [HttpDelete("{id}")]
        public RespostaAPI DeleteImagem(int id) {
            Repository Repository = new();

            var Imagem = Repository.FilterQuery<Anexo>((p) => p.Anexo1 == id).FirstOrDefault();

            if (Imagem is null) {
                return new RespostaAPI("Registro não encontrado");
            }

            var QtdImagensProduto = Repository.FilterQuery<Anexo>(p => p.Produto == Imagem.Produto).Count();

            if (QtdImagensProduto == 1) {
                return new RespostaAPI("Não é possível excluir todas as imagens do produto");
            }

            Repository.Delete(Imagem);
            Repository.Dispose();

            return new RespostaAPI(new { sucesso = true });
        }
    }
}
