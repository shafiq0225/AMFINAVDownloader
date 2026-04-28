import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { PermissionGuard } from '../../core/guards/permission.guard';

import { EmployeeDashboardComponent } from './dashboard/dashboard.component';
import { EmployeeSchemesComponent } from './schemes/schemes.component';
import { EmployeeNavComponent } from './nav/nav.component';

const routes: Routes = [
  { path: 'dashboard', component: EmployeeDashboardComponent },
  { path: 'schemes', component: EmployeeSchemesComponent },
  {
    path: 'nav',
    component: EmployeeNavComponent,
    canActivate: [PermissionGuard],
    data: { permission: 'nav.read' }
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];

@NgModule({
  declarations: [
    EmployeeDashboardComponent,
    EmployeeSchemesComponent,
    EmployeeNavComponent
  ],
  imports: [
    SharedModule,            // ← this brings in CommonModule, ReactiveFormsModule,
    RouterModule.forChild(routes)  //   NavChartComponent, LoadingSpinnerComponent
  ]
})
export class EmployeeModule { }