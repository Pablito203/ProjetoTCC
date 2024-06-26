import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AlertOptions  } from '@ionic/angular';
import { AlertaService } from 'src/app/services/alerta/alerta.service';

@Component({
  selector: 'produto-card-lista',
  templateUrl: './produto-card-lista.component.html',
  styleUrls: ['./produto-card-lista.component.scss'],
})
export class ProdutoCardListaComponent {
  salvando = false;
  @Input() Tipo: String = "";
  @Input() Produto: any = {};
  @Output() Excluir: EventEmitter<void> = new EventEmitter<void>;
  @Output() Alterado: EventEmitter<void> = new EventEmitter<void>;

  TimeoutQuantidade: any;

  constructor(private AlertaService: AlertaService) { }

  SubtrairClick() {
    if (!this.Produto.quantidade || this.Produto.quantidade <= 1) {return;}
    this.Produto.quantidade -= 1;
    this.EmitirAlterado();
  }

  AdicionarClick() {
    if (!this.Produto.quantidade) {return;}
    this.Produto.quantidade += 1;
    this.EmitirAlterado();
  }

  EmitirAlterado() {
    if (this.TimeoutQuantidade) {
      clearTimeout(this.TimeoutQuantidade);
    }

    this.TimeoutQuantidade = setTimeout(() => {
      this.Alterado.emit();
    }, 400);
  }

  ExcluirClick() {
    if (this.salvando) {return;}
    this.salvando = true;

    const AlertaOptions: AlertOptions =  {};
    AlertaOptions.header = 'Excluir';
    AlertaOptions.message = 'Deseja excluir o produto ' + (this.Tipo === 'Favoritos' ? 'dos favoritos' : 'da sacola') + '?';
    AlertaOptions.buttons = [
      {
        text: 'Cancelar',
        handler: () => { this.salvando = false; }
      },
      {
        text: 'Excluir',
        handler: () => {
          this.Excluir.emit();
          this.salvando = false;
        }
      }
    ]

    this.AlertaService.CriarAlerta(AlertaOptions);
  }

}
