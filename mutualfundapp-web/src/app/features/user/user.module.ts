import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { PermissionGuard } from '../../core/guards/permission.guard';

import { UserDashboardComponent } from './dashboard/dashboard.component';
import { NavViewComponent } from './nav-view/nav-view.component';
import { FamilyViewComponent } from './family-view/family-view.component';

const routes: Routes = [
  { path: 'dashboard', component: UserDashboardComponent },
  {
    path: 'nav',
    component: NavViewComponent,
    canActivate: [PermissionGuard],
    data: { permission: 'nav.read' }
  },
  { path: 'family', component: FamilyViewComponent },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];

@NgModule({
  declarations: [
    UserDashboardComponent,
    NavViewComponent,        // ← only declared here
    FamilyViewComponent
  ],
  imports: [
    SharedModule,
    RouterModule.forChild(routes)
  ]
})
export class UserModule { }