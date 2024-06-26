import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { IonicModule } from '@ionic/angular';

import { AddProdutoPageRoutingModule } from './add-produto-routing.module';

import { AddProdutoPage } from './add-produto.page';
import { ComponentsModule } from "../../components/components.module";
import { DirectivesModule } from 'src/app/directives/directives.module';

@NgModule({
    declarations: [AddProdutoPage],
    imports: [
        CommonModule,
        FormsModule,
        IonicModule,
        AddProdutoPageRoutingModule,
        ComponentsModule,
        DirectivesModule
    ]
})
export class AddProdutoPageModule {}
