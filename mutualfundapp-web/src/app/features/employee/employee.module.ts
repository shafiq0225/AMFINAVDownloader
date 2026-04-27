import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';

import { EmployeeDashboardComponent } from './dashboard/dashboard.component';
import { EmployeeSchemesComponent }   from './schemes/schemes.component';

const routes: Routes = [
  { path: 'dashboard', component: EmployeeDashboardComponent },
  { path: 'schemes',   component: EmployeeSchemesComponent },
  { path: '',          redirectTo: 'dashboard', pathMatch: 'full' }
];

@NgModule({
  declarations: [
    EmployeeDashboardComponent,
    EmployeeSchemesComponent
  ],
  imports: [
    SharedModule,
    RouterModule.forChild(routes)
  ]
})
export class EmployeeModule { }