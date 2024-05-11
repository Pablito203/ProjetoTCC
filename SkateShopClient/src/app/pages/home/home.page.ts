import { NavController } from '@ionic/angular';
import { AlertaService } from './../../services/alerta/alerta.service';
import { ProdutoService } from './../../services/produto/produto.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'home',
  templateUrl: './home.page.html',
  styleUrls: ['./home.page.scss'],
})
export class HomePage implements OnInit {

  lancamentos: any = [];
  destaques: any = [];
  documentElement: any;
  math = Math;

  constructor(private produtoService: ProdutoService,
              private alertaService: AlertaService,
              private navController: NavController) { }

  ngOnInit() {
    this.documentElement = document.documentElement;
    setTimeout(() => {
      this.produtoService.GetLancamentos().subscribe((data: any) => {
        if (data.mensagemErro) {
          this.alertaService.CriarToastMensagem(data.mensagemErro, true);
          return;
        }

        this.lancamentos = data.result;
      });

      this.produtoService.GetDestaques().subscribe((data: any) => {
        if (data.mensagemErro) {
          this.alertaService.CriarToastMensagem(data.mensagemErro, true);
          return;
        }

        this.destaques = data.result;
      });
    }, 100);
  }

  navigate(url: string) {
    this.navController.navigateForward(url);
  }
}
