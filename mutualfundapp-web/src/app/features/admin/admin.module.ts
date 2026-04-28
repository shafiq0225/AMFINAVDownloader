import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';

import { DashboardComponent } from './dashboard/dashboard.component';
import { UsersComponent } from './users/users.component';
import { PendingComponent } from './users/pending/pending.component';
import { FamilyComponent } from './family/family.component';
import { SchemesComponent } from './schemes/schemes.component';
import { AdminNavComponent } from './nav/nav.component';
import { NavGrowCountPipe } from "../../shared/pipes/nav-grow-count.pipe";

const routes: Routes = [
  { path: 'dashboard', component: DashboardComponent },
  { path: 'users', component: UsersComponent },
  { path: 'users/pending', component: PendingComponent },
  { path: 'family', component: FamilyComponent },
  { path: 'schemes', component: SchemesComponent },
  { path: 'nav', component: AdminNavComponent },  // ← add
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];

@NgModule({
  declarations: [
    DashboardComponent,
    UsersComponent,
    PendingComponent,
    FamilyComponent,
    SchemesComponent,
    AdminNavComponent
  ],
  imports: [
    SharedModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes),
]
})
export class AdminModule { }