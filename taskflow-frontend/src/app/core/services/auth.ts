
import { inject, Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import {
  LoginRequest,
  LoginResponse
} from '../../shared/models/login.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = '/api/auth';

  private readonly accessToken = signal<string | null>(null);
  private readonly expiresAt = signal<number | null>(null);

  readonly isAuthenticated = computed(() =>
    this.accessToken() !== null &&
    this.expiresAt() !== null &&
    Date.now() < this.expiresAt()!
  );

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      `${this.apiUrl}/login`,
      credentials
    ).pipe(
      tap(response => {
        this.accessToken.set(response.token);

        this.expiresAt.set(
          Date.now() + response.expiresIn * 1000
        );
      })
    );
  }

  getToken(): string | null {
    const token = this.accessToken();
    const expiration = this.expiresAt();

    if (!token || !expiration || Date.now() >= expiration) {
      this.logout();
      return null;
    }

    return token;
  }

  logout(): void {
    this.accessToken.set(null);
    this.expiresAt.set(null);
  }
}
