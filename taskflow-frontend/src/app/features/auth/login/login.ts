
import { Component, inject } from '@angular/core';
import {
  Language,
  LanguageService
} from '../../../core/services/language';
import { ThemeService } from '../../../core/services/theme';

@Component({
  selector: 'app-login',
  imports: [],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  readonly languageService = inject(LanguageService);
  readonly themeService = inject(ThemeService);

  changeLanguage(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.languageService.setLanguage(select.value as Language);
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  onSubmit(event: Event): void {
    event.preventDefault();
    // A autenticação será integrada à API na próxima etapa.
  }
}
