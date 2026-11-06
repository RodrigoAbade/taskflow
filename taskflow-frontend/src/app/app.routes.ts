
import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login')
        .then(m => m.Login),
    title: 'Login | TaskFlow'
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/dashboard/dashboard')
        .then(m => m.Dashboard),
    title: 'Dashboard | TaskFlow'
  },
  {
    path: 'projects',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/projects/projects/projects')
        .then(m => m.Projects),
    title: 'Projetos | TaskFlow'
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
