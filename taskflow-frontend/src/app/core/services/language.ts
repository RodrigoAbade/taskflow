
import { Injectable, signal } from '@angular/core';

export type Language = 'pt' | 'en' | 'es';

type TranslationKey =
  | 'welcome'
  | 'loginSubtitle'
  | 'email'
  | 'password'
  | 'emailPlaceholder'
  | 'passwordPlaceholder'
  | 'loginButton'
  | 'loginFooter'
  | 'language'
  | 'theme';

const translations: Record<
  Language,
  Record<TranslationKey, string>
> = {
  pt: {
    welcome: 'Bem-vindo de volta',
    loginSubtitle: 'Entre na sua conta para continuar.',
    email: 'E-mail',
    password: 'Senha',
    emailPlaceholder: 'seu@email.com',
    passwordPlaceholder: 'Digite sua senha',
    loginButton: 'Entrar',
    loginFooter: 'Organize seu trabalho. Simplifique seu dia.',
    language: 'Idioma',
    theme: 'Tema'
  },

  en: {
    welcome: 'Welcome back',
    loginSubtitle: 'Sign in to your account to continue.',
    email: 'Email',
    password: 'Password',
    emailPlaceholder: 'your@email.com',
    passwordPlaceholder: 'Enter your password',
    loginButton: 'Sign in',
    loginFooter: 'Organize your work. Simplify your day.',
    language: 'Language',
    theme: 'Theme'
  },

  es: {
    welcome: 'Bienvenido de nuevo',
    loginSubtitle: 'Inicia sesión para continuar.',
    email: 'Correo electrónico',
    password: 'Contraseña',
    emailPlaceholder: 'tu@email.com',
    passwordPlaceholder: 'Introduce tu contraseña',
    loginButton: 'Iniciar sesión',
    loginFooter: 'Organiza tu trabajo. Simplifica tu día.',
    language: 'Idioma',
    theme: 'Tema'
  }
};

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  private readonly storageKey = 'taskflow-language';

  readonly language = signal<Language>(
    this.getInitialLanguage()
  );

  constructor() {
    this.applyLanguage(this.language());
  }

  private getInitialLanguage(): Language {
    const saved = localStorage.getItem(this.storageKey);

    if (saved === 'pt' || saved === 'en' || saved === 'es') {
      return saved;
    }

    return 'pt';
  }

  setLanguage(language: Language): void {
    this.language.set(language);
    localStorage.setItem(this.storageKey, language);
    this.applyLanguage(language);
  }

  translate(key: TranslationKey): string {
    return translations[this.language()][key];
  }

  private applyLanguage(language: Language): void {
    document.documentElement.lang = language;
  }
}
