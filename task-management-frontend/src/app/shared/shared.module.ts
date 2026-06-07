import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AlertMessageComponent } from './components/alert-message/alert-message.component';

@NgModule({
  declarations: [
    AlertMessageComponent 
  ],
  imports: [
    CommonModule 
  ],
  exports: [
    AlertMessageComponent
  ]
})
export class SharedModule { }