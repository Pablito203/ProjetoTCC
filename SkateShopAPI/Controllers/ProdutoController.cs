﻿using Microsoft.AspNetCore.Mvc;
using SkateShopAPI.EntityModels;
using SkateShopAPI.ModelsAPI;
using SkateShopAPI.Services;

namespace SkateShopAPI.Controllers {
    [ApiController]
    [Route("api")]
    public class ProdutoController : ControllerBase {
        
        [HttpGet("[Controller]")]
        public RespostaAPI GetProduto() {
            Repository Repository = new();
            var iqProduto = Repository.FilterQuery<Produto>((p) => true);
            SetIQueryableProduto(ref iqProduto);

            var lstProduto = iqProduto.Select((p) => new ProdutoRetorno() {
                ProdutoID = p.Produto1,
                Nome = p.Nome,
                Valor = p.Valor,
                Ativo = p.Ativo,
                Destaque = p.Destaque,
                QuantidadeEstoque = p.QuantidadeEstoque,
                TamanhoUnico = p.TamanhoUnico,
                CaminhoImagem = p.Anexos.Select((p) => p.CaminhoRelativo).FirstOrDefault(),
            }).ToList();

            List<int> lstProdutoFavoritoID = null;

            if (Request.Headers.TryGetValue("UsuarioID", out var strUsuarioID)) {
                int UsuarioID = int.Parse(strUsuarioID);
                lstProdutoFavoritoID = Repository.FilterQuery<Favorito>((p) => p.Usuario == UsuarioID).Select(p => p.Produto).ToList();
            }

            Repository.Dispose();

            foreach (var Produto in lstProduto) {
                if (Produto.CaminhoImagem is not null) {
                    Produto.CaminhoImagem = AnexoService.BuscarArquivoBase64(Produto.CaminhoImagem);
                }

                if (lstProdutoFavoritoID is not null) {
                    Produto.Favorito = lstProdutoFavoritoID.Contains(Produto.ProdutoID);
                }
            }

            return new RespostaAPI(lstProduto);
        }

        [HttpPost("[Controller]")]
        public RespostaAPI PostProduto(ProdutoBody ProdutoBody) {
            Produto Produto = new Produto() {
                Nome = ProdutoBody.Nome,
                Valor = ProdutoBody.Valor,
                Destaque = ProdutoBody.Destaque,
                QuantidadeEstoque = ProdutoBody.QuantidadeEstoque,
                TamanhoUnico = ProdutoBody.TamanhoUnico,
                DataCriacao = DateTime.Now
            };

            Repository Repository = new();
            Repository.Insert(Produto);
            Repository.Dispose();

            ProdutoRetorno ProdutoRetorno = new ProdutoRetorno()
            {
                ProdutoID = Produto.Produto1,
                Nome = Produto.Nome,
                Valor = Produto.Valor,
                Destaque = Produto.Destaque,
                QuantidadeEstoque = Produto.QuantidadeEstoque,
                TamanhoUnico = Produto.TamanhoUnico
            };

            return new RespostaAPI(ProdutoRetorno);
        }

        [HttpPut("[Controller]")]
        public RespostaAPI PutProduto(ProdutoBody ProdutoBody)
        {
            if (!ProdutoBody.ProdutoID.HasValue) {
                return new RespostaAPI("Registro não encontrado");
            }

            Repository Repository = new();
            var Produto = Repository.FilterQuery<Produto>((p) => p.Produto1 == ProdutoBody.ProdutoID).FirstOrDefault();

            if (Produto is null) {
                return new RespostaAPI("Registro não encontrado");
            }

            Produto.Produto1 = ProdutoBody.ProdutoID.Value;
            Produto.Nome = ProdutoBody.Nome;
            Produto.Valor = ProdutoBody.Valor;
            Produto.Destaque = ProdutoBody.Destaque;
            Produto.QuantidadeEstoque = ProdutoBody.QuantidadeEstoque;
            Produto.TamanhoUnico = ProdutoBody.TamanhoUnico;

            Repository.Dispose();
            Repository.Update(Produto);

            ProdutoRetorno ProdutoRetorno = new ProdutoRetorno()
            {
                ProdutoID = Produto.Produto1,
                Nome = Produto.Nome,
                Valor = Produto.Valor,
                Destaque = Produto.Destaque,
                QuantidadeEstoque = Produto.QuantidadeEstoque,
                TamanhoUnico = Produto.TamanhoUnico
            };

            return new RespostaAPI(ProdutoRetorno);
        }

        [HttpDelete("[Controller]/{id}")]
        public RespostaAPI DeleteProduto(int id) {
            Repository Repository = new();
            var ProdutoDados = Repository.FilterQuery<Produto>((p) => p.Produto1 == id).Select((p) => new {
                Produto = p,
                PossuiPedido = p.PedidoProdutos.Any(),
            }).FirstOrDefault();

            if (ProdutoDados is null) {
                return new RespostaAPI("Registro não encontrado");
            }
            
            if (ProdutoDados.PossuiPedido) {
                ProdutoDados.Produto.Ativo = false;
                Repository.Update(ProdutoDados.Produto);
                return new RespostaAPI(new { sucesso = true }, "O produto não foi excluído por ter pedidos relacionados, o produto foi inativado");
            }
            var lstTamanhoProduto = Repository.FilterQuery<Tamanho>((p) => p.Produto == id).ToList();
            var lstAnexo = Repository.FilterQuery<Anexo>((p) => p.Produto == id).ToList();

            Repository.Delete(lstTamanhoProduto);
            Repository.Delete(lstAnexo);
            Repository.Delete(ProdutoDados.Produto);
            Repository.Dispose();

            return new RespostaAPI(new { sucesso = true });
        }

        [HttpGet("[Controller]ID")]
        public RespostaAPI GetProdutoID() {
            Repository Repository = new();
            var lstProdutoID = Repository.FilterQuery<Produto>((p) => true).Select((p) => p.Produto1).ToList();
            Repository.Dispose();
            return new RespostaAPI(lstProdutoID);
        }


        private void SetIQueryableProduto(ref IQueryable<Produto> iqProduto) {
            Request.Headers.TryGetValue("tipo", out var Tipo);

            switch (Tipo) {
                case "lancamentos":
                    iqProduto = iqProduto.OrderByDescending(p => p.DataCriacao).Take(10);
                    break;
                case "destaques":
                    iqProduto = iqProduto.Where((p) => p.Destaque);
                    break;
                default:
                    if (!Request.Headers.TryGetValue("ListaID", out var ListaID)) {
                        iqProduto = iqProduto.Take(30);
                        break;
                    }

                    var lstProdutoID = ListaID.First().Split(",").Select((p) => int.Parse(p)).ToList();
                    if (lstProdutoID.Count() == 1) {
                        int ProdutoID = lstProdutoID.First();
                        iqProduto = iqProduto.Where((p) => p.Produto1 == ProdutoID);
                        break;
                    }
                    iqProduto = iqProduto.Where((p) => lstProdutoID.Contains(p.Produto1));
                    break;
            }
        }
    }
}
