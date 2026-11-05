
import { inject } from '@angular/core';
import {
  HttpInterceptorFn
} from '@angular/common/http';

import { AuthService } from '../services/auth';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  // Somente requisições para a API do TaskFlow.
  if (!req.url.startsWith('/api/')) {
    return next(req);
  }

  // O login não precisa de autenticação.
  if (req.url === '/api/auth/login') {
    return next(req);
  }

  const token = authService.getToken();

  if (!token) {
    return next(req);
  }

  const authenticatedRequest = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(authenticatedRequest);
};
