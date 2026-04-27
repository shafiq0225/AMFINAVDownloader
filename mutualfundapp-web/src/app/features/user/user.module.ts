import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';

import { UserDashboardComponent } from './dashboard/dashboard.component';
import { NavViewComponent } from './nav-view/nav-view.component';
import { FamilyViewComponent } from './family-view/family-view.component';

const routes: Routes = [
  { path: 'dashboard', component: UserDashboardComponent },
  { path: 'nav', component: NavViewComponent },
  { path: 'family', component: FamilyViewComponent },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];

@NgModule({
  declarations: [
    UserDashboardComponent,
    NavViewComponent,
    FamilyViewComponent
  ],
  imports: [
    SharedModule,
    RouterModule.forChild(routes)
  ]
})
export class UserModule { }