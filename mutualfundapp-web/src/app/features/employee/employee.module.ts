import { NgModule, Component } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { CommonModule } from '@angular/common';

@Component({
  template: `
    <div class="page-header">
      <h2>Employee Dashboard</h2>
      <p>Coming soon — Step 10</p>
    </div>
  `,
  standalone:false
})
export class DashboardPlaceholderComponent {} // ✅ FIXED

const routes: Routes = [
  {
    path: 'dashboard',
    component: DashboardPlaceholderComponent
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];

@NgModule({
  declarations: [DashboardPlaceholderComponent],
  imports: [CommonModule, SharedModule, RouterModule.forChild(routes)]
})
export class EmployeeModule {}