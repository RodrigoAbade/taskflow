
import {
  Component,
  computed,
  inject,
  OnInit,
  signal
} from '@angular/core';

import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import {
  Language,
  LanguageService
} from '../../../core/services/language';

import { ThemeService } from '../../../core/services/theme';
import { AuthService } from '../../../core/services/auth';
import { ProjectService } from '../../../core/services/project';
import { Project } from '../../../shared/models/project.model';

type ModalTextKey =
  | 'createTitle'
  | 'createSubtitle'
  | 'projectName'
  | 'projectDescription'
  | 'namePlaceholder'
  | 'descriptionPlaceholder'
  | 'nameRequired'
  | 'nameTooLong'
  | 'cancel'
  | 'saveProject'
  | 'saving'
  | 'createError'
  | 'createSuccess'
  | 'close';

const modalTranslations: Record<
  Language,
  Record<ModalTextKey, string>
> = {
  pt: {
    createTitle: 'Novo projeto',
    createSubtitle: 'Organize seu trabalho em um novo projeto.',
    projectName: 'Nome do projeto',
    projectDescription: 'Descrição',
    namePlaceholder: 'Ex.: Meu novo projeto',
    descriptionPlaceholder: 'Descreva o objetivo do projeto...',
    nameRequired: 'Informe o nome do projeto.',
    nameTooLong: 'O nome deve ter no máximo 100 caracteres.',
    cancel: 'Cancelar',
    saveProject: 'Criar projeto',
    saving: 'Salvando...',
    createError: 'Não foi possível criar o projeto. Tente novamente.',
    createSuccess: 'Projeto criado com sucesso!',
    close: 'Fechar'
  },
  en: {
    createTitle: 'New project',
    createSubtitle: 'Organize your work in a new project.',
    projectName: 'Project name',
    projectDescription: 'Description',
    namePlaceholder: 'E.g. My new project',
    descriptionPlaceholder: 'Describe the project goal...',
    nameRequired: 'Enter a project name.',
    nameTooLong: 'The name must have at most 100 characters.',
    cancel: 'Cancel',
    saveProject: 'Create project',
    saving: 'Saving...',
    createError: 'Could not create the project. Please try again.',
    createSuccess: 'Project created successfully!',
    close: 'Close'
  },
  es: {
    createTitle: 'Nuevo proyecto',
    createSubtitle: 'Organiza tu trabajo en un nuevo proyecto.',
    projectName: 'Nombre del proyecto',
    projectDescription: 'Descripción',
    namePlaceholder: 'Ej.: Mi nuevo proyecto',
    descriptionPlaceholder: 'Describe el objetivo del proyecto...',
    nameRequired: 'Introduce el nombre del proyecto.',
    nameTooLong: 'El nombre debe tener como máximo 100 caracteres.',
    cancel: 'Cancelar',
    saveProject: 'Crear proyecto',
    saving: 'Guardando...',
    createError: 'No se pudo crear el proyecto. Inténtalo de nuevo.',
    createSuccess: '¡Proyecto creado correctamente!',
    close: 'Cerrar'
  }
};

@Component({
  selector: 'app-dashboard',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  readonly languageService = inject(LanguageService);
  readonly themeService = inject(ThemeService);

  private readonly authService = inject(AuthService);
  private readonly projectService = inject(ProjectService);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);

  readonly mobileMenuOpen = signal(false);

  readonly projects = signal<Project[]>([]);
  readonly projectsLoading = signal(true);
  readonly projectsError = signal(false);

  readonly createProjectModalOpen = signal(false);
  readonly projectSaving = signal(false);
  readonly projectSaveError = signal(false);
  readonly projectSuccess = signal(false);

  readonly projectForm = this.formBuilder.nonNullable.group({
    name: [
      '',
      [
        Validators.required,
        Validators.maxLength(100)
      ]
    ],
    description: ['']
  });

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

  modalText(key: ModalTextKey): string {
    return modalTranslations[
      this.languageService.language()
    ][key];
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

  openCreateProject(): void {
    this.projectForm.reset();
    this.projectSaveError.set(false);
    this.projectSuccess.set(false);
    this.createProjectModalOpen.set(true);
  }

  closeCreateProject(): void {
    if (this.projectSaving()) {
      return;
    }

    this.createProjectModalOpen.set(false);
    this.projectSaveError.set(false);
    this.projectForm.reset();
  }

  createProject(): void {
    if (this.projectSaving()) {
      return;
    }

    const name = this.projectForm.controls.name.value.trim();
    const description =
      this.projectForm.controls.description.value.trim();

    this.projectForm.controls.name.setValue(name);

    if (!name || name.length > 100) {
      this.projectForm.controls.name.markAsTouched();
      return;
    }

    if (this.projectForm.invalid) {
      this.projectForm.markAllAsTouched();
      return;
    }

    this.projectSaving.set(true);
    this.projectSaveError.set(false);

    this.projectService.create({
      name,
      description
    }).subscribe({
      next: (project) => {
        this.projectSaving.set(false);
        this.createProjectModalOpen.set(false);
        this.projectForm.reset();

        this.projects.update(current => {
          const withoutDuplicate = current.filter(
            item => item.id !== project.id
          );

          return [project, ...withoutDuplicate];
        });

        this.projectsError.set(false);
        this.projectsLoading.set(false);
        this.projectSuccess.set(true);
      },

      error: (error: HttpErrorResponse) => {
        console.error('Erro ao criar projeto:', error);
        this.projectSaving.set(false);
        this.projectSaveError.set(true);
      }
    });
  }

  dismissSuccess(): void {
    this.projectSuccess.set(false);
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
    this.createProjectModalOpen.set(false);

    void this.router.navigate(['/login']);
  }
}
