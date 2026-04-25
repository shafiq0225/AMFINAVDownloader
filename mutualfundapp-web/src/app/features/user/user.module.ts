import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { Component } from '@angular/core';

@Component({
  template: `
    <div class="page-header">
      <h2>User Dashboard</h2>
      <p>Coming soon — Step 11</p>
    </div>
  `,
  standalone:false
})
export class UserDashboardPlaceholderComponent {}

const routes: Routes = [
  { path: 'dashboard', component: UserDashboardPlaceholderComponent },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];

@NgModule({
  declarations: [UserDashboardPlaceholderComponent],
  imports: [SharedModule, RouterModule.forChild(routes)]
})
export class UserModule { }