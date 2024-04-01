import { Injectable } from '@angular/core';
import { ApiService } from '../api/api.service';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class ImagemService {

  constructor(private http: HttpClient) { }

  PostImagem(Imagem: any) {
    let formData = new FormData();
    formData.append('arquivo', Imagem.Arquivo, Imagem.nome);
    formData.append('ProdutoID', Imagem.ProdutoID);
    formData.append('Nome', Imagem.Nome);

    return this.http.post(ApiService.url + 'Imagem', formData);
  }
}
