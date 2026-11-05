
import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { finalize } from 'rxjs';

import {
  Language,
  LanguageService
} from '../../../core/services/language';
import { ThemeService } from '../../../core/services/theme';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  readonly languageService = inject(LanguageService);
  readonly themeService = inject(ThemeService);

  private readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  private readonly messages = {
    pt: {
      required: 'Preencha todos os campos.',
      email: 'Digite um e-mail válido.',
      invalid: 'E-mail ou senha inválidos.',
      connection: 'Não foi possível conectar à API.',
      success: 'Login realizado com sucesso!'
    },
    en: {
      required: 'Please fill in all fields.',
      email: 'Enter a valid email address.',
      invalid: 'Invalid email or password.',
      connection: 'Could not connect to the API.',
      success: 'Successfully signed in!'
    },
    es: {
      required: 'Completa todos los campos.',
      email: 'Introduce un correo válido.',
      invalid: 'Correo o contraseña incorrectos.',
      connection: 'No se pudo conectar con la API.',
      success: '¡Inicio de sesión exitoso!'
    }
  };

  changeLanguage(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.languageService.setLanguage(select.value as Language);
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  onSubmit(): void {
    this.errorMessage.set('');
    this.successMessage.set('');

    const language = this.languageService.language();
    const messages = this.messages[language];

    if (this.form.invalid) {
      this.form.markAllAsTouched();

      if (this.form.controls.email.hasError('email')) {
        this.errorMessage.set(messages.email);
      } else {
        this.errorMessage.set(messages.required);
      }

      return;
    }

    if (this.loading()) return;

    this.loading.set(true);

    this.authService.login(this.form.getRawValue())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => {
          console.log('Autenticação concluída.');

          this.successMessage.set(
            this.messages[this.languageService.language()].success
          );

          // Na próxima etapa, trataremos o token JWT
          // e o redirecionamento para o dashboard.
        },

        error: (error: HttpErrorResponse) => {
          const currentMessages =
            this.messages[this.languageService.language()];

          if (error.status === 401) {
            this.errorMessage.set(currentMessages.invalid);
          } else {
            this.errorMessage.set(currentMessages.connection);
          }
        }
      });
  }
}
