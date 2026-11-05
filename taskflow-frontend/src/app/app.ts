import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { inject } from '@angular/core';
import { ThemeService } from './core/services/theme';
import { LanguageService } from './core/services/language';

@Component({
  imports: [RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  protected readonly title = signal('taskflow');
  protected readonly themeService = inject(ThemeService);
  protected readonly languageService = inject(LanguageService);
}
