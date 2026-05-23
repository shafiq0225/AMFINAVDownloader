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
import { OrdersComponent } from './investment/orders/orders.component';
import { OrderDetailComponent } from './investment/order-detail/order-detail.component';
import { PortfolioOverviewComponent } from './investment/portfolio-overview/portfolio-overview.component';
import { SchemeDetailsComponent } from './scheme-details/scheme-details.component';
import { MemberPortfolioComponent } from './investment/member-portfolio/member-portfolio.component';

const routes: Routes = [
  { path: 'dashboard', component: DashboardComponent },
  { path: 'users', component: UsersComponent },
  { path: 'users/pending', component: PendingComponent },
  { path: 'family', component: FamilyComponent },
  { path: 'schemes', component: SchemesComponent },
  { path: 'nav', component: AdminNavComponent },
  { path: 'orders', component: OrdersComponent },
  { path: 'orders/:id', component: OrderDetailComponent },
  // ── Portfolio routes (specific before generic) ──────────────
  { path: 'portfolio/member/:userId', component: MemberPortfolioComponent }, // ← NEW
  { path: 'portfolio', component: PortfolioOverviewComponent },
  // ────────────────────────────────────────────────────────────
  { path: 'nav/scheme/:schemeCode', component: SchemeDetailsComponent },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];

@NgModule({
  declarations: [
    DashboardComponent,
    UsersComponent,
    PendingComponent,
    FamilyComponent,
    SchemesComponent,
    AdminNavComponent,
    OrdersComponent,
    OrderDetailComponent,
    PortfolioOverviewComponent,
    MemberPortfolioComponent,
    SchemeDetailsComponent
  ],
  imports: [
    SharedModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes)
  ]
})
export class AdminModule { }