
import {
  Component,
  computed,
  inject,
  OnInit,
  signal
} from '@angular/core';

import { Router } from '@angular/router';

import {
  Language,
  LanguageService
} from '../../../core/services/language';

import { ThemeService } from '../../../core/services/theme';
import { AuthService } from '../../../core/services/auth';
import { ProjectService } from '../../../core/services/project';
import { Project } from '../../../shared/models/project.model';

@Component({
  selector: 'app-dashboard',
  imports: [],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  readonly languageService = inject(LanguageService);
  readonly themeService = inject(ThemeService);

  private readonly authService = inject(AuthService);
  private readonly projectService = inject(ProjectService);
  private readonly router = inject(Router);

  readonly mobileMenuOpen = signal(false);

  readonly projects = signal<Project[]>([]);
  readonly projectsLoading = signal(true);
  readonly projectsError = signal(false);

  readonly activeProjects = computed(() =>
    this.projects().filter(project => project.isActive)
  );

  readonly recentProjects = computed(() =>
    [...this.activeProjects()]
      .sort(
        (a, b) =>
          new Date(b.createdAt).getTime() -
          new Date(a.createdAt).getTime()
      )
      .slice(0, 5)
  );

  readonly statistics = [
    { key: 'totalProjects', icon: 'folder' },
    { key: 'totalTasks', icon: 'tasks' },
    { key: 'inProgress', icon: 'progress' },
    { key: 'completed', icon: 'completed' }
  ] as const;

  ngOnInit(): void {
    this.loadProjects();
  }

  private loadProjects(): void {
    this.projectsLoading.set(true);
    this.projectsError.set(false);

    this.projectService.getAll().subscribe({
      next: (projects) => {
        this.projects.set(projects);
        this.projectsLoading.set(false);
      },

      error: (error) => {
        console.error('Erro ao carregar projetos:', error);

        this.projectsError.set(true);
        this.projectsLoading.set(false);
      }
    });
  }

  formatDate(date: string): string {
    const locale = {
      pt: 'pt-BR',
      en: 'en-US',
      es: 'es-ES'
    }[this.languageService.language()];

    const parsedDate = new Date(date);

    if (Number.isNaN(parsedDate.getTime())) {
      return '—';
    }

    return new Intl.DateTimeFormat(locale, {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).format(parsedDate);
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update(value => !value);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  changeLanguage(event: Event): void {
    const select = event.target as HTMLSelectElement;

    this.languageService.setLanguage(
      select.value as Language
    );
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  logout(): void {
    this.authService.logout();
    this.projects.set([]);

    void this.router.navigate(['/login']);
  }
}
