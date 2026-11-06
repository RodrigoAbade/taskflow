
import {
  inject,
  Injectable,
  signal,
  computed
} from '@angular/core';

import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import {
  LoginRequest,
  LoginResponse
} from '../../shared/models/login.model';

interface StoredSession {
  token: string;
  expiresAt: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = '/api/auth';
  private readonly storageKey = 'taskflow-session';

  private readonly initialSession = this.restoreSession();

  private readonly accessToken = signal<string | null>(
    this.initialSession?.token ?? null
  );

  private readonly expiresAt = signal<number | null>(
    this.initialSession?.expiresAt ?? null
  );

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
        const expiration =
          Date.now() + response.expiresIn * 1000;

        this.accessToken.set(response.token);
        this.expiresAt.set(expiration);

        const session: StoredSession = {
          token: response.token,
          expiresAt: expiration
        };

        sessionStorage.setItem(
          this.storageKey,
          JSON.stringify(session)
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

  private restoreSession(): StoredSession | null {
    const savedSession = sessionStorage.getItem(
      this.storageKey
    );

    if (!savedSession) {
      return null;
    }

    try {
      const session: unknown = JSON.parse(savedSession);

      if (
        typeof session !== 'object' ||
        session === null ||
        !('token' in session) ||
        !('expiresAt' in session) ||
        typeof session.token !== 'string' ||
        !session.token ||
        typeof session.expiresAt !== 'number' ||
        !Number.isFinite(session.expiresAt) ||
        Date.now() >= session.expiresAt
      ) {
        sessionStorage.removeItem(this.storageKey);
        return null;
      }

      return {
        token: session.token,
        expiresAt: session.expiresAt
      };
    } catch {
      sessionStorage.removeItem(this.storageKey);
      return null;
    }
  }

  logout(): void {
    this.accessToken.set(null);
    this.expiresAt.set(null);

    sessionStorage.removeItem(this.storageKey);
  }
}
