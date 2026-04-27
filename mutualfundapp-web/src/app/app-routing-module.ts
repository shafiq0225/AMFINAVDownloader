import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';

const routes: Routes = [
  // ── Public (no layout) ────────────────────────────────────────
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.module').then(m => m.AuthModule)
  },

  // ── Protected (wrapped in sidebar + topbar) ───────────────────
  {
    path: '',
    component: MainLayoutComponent,   // ← layout shell wraps ALL protected routes
    canActivate: [AuthGuard],
    children: [
      {
        path: 'admin',
        canActivate: [RoleGuard],
        data: { roles: ['Admin'] },
        loadChildren: () =>
          import('./features/admin/admin.module').then(m => m.AdminModule)
      },
      {
        path: 'employee',
        canActivate: [RoleGuard],
        data: { roles: ['Employee'] },
        loadChildren: () =>
          import('./features/employee/employee.module')
            .then(m => m.EmployeeModule)
      },
      {
        path: 'user',
        canActivate: [RoleGuard],
        data: { roles: ['User'] },
        loadChildren: () =>
          import('./features/user/user.module').then(m => m.UserModule)
      }
    ]
  },

  { path: '**', redirectTo: 'auth/login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }