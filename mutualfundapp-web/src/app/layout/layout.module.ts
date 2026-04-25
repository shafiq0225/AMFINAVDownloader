import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MainLayoutComponent } from './main-layout/main-layout.component';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [MainLayoutComponent],
  imports: [
    SharedModule,
    RouterModule
  ],
  exports: [MainLayoutComponent]
})
export class LayoutModule { }