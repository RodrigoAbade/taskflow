
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

type TextKey =
  | 'subtitle'
  | 'active'
  | 'inactive'
  | 'all'
  | 'created'
  | 'loading'
  | 'loadError'
  | 'retry'
  | 'edit'
  | 'deactivate'
  | 'confirmDeactivate'
  | 'confirmDescription'
  | 'cancel'
  | 'confirm'
  | 'createTitle'
  | 'editTitle'
  | 'name'
  | 'description'
  | 'namePlaceholder'
  | 'descriptionPlaceholder'
  | 'nameRequired'
  | 'nameTooLong'
  | 'save'
  | 'create'
  | 'saving'
  | 'saveError'
  | 'deactivateError'
  | 'createdSuccess'
  | 'updatedSuccess'
  | 'deactivatedSuccess'
  | 'close'
  | 'empty'
  | 'noDescription';

const translations: Record<Language, Record<TextKey, string>> = {
  pt: {
    subtitle: 'Organize, acompanhe e gerencie seus projetos.',
    active: 'Ativos',
    inactive: 'Inativos',
    all: 'Todos',
    created: 'Criado em',
    loading: 'Carregando projetos...',
    loadError: 'Não foi possível carregar os projetos.',
    retry: 'Tentar novamente',
    edit: 'Editar',
    deactivate: 'Desativar',
    confirmDeactivate: 'Desativar projeto',
    confirmDescription: 'Este projeto ficará inativo. Deseja continuar?',
    cancel: 'Cancelar',
    confirm: 'Confirmar desativação',
    createTitle: 'Novo projeto',
    editTitle: 'Editar projeto',
    name: 'Nome do projeto',
    description: 'Descrição',
    namePlaceholder: 'Ex.: Meu projeto',
    descriptionPlaceholder: 'Descreva o objetivo...',
    nameRequired: 'Informe o nome do projeto.',
    nameTooLong: 'Máximo de 100 caracteres.',
    save: 'Salvar alterações',
    create: 'Criar projeto',
    saving: 'Salvando...',
    saveError: 'Não foi possível salvar o projeto.',
    deactivateError: 'Não foi possível desativar o projeto.',
    createdSuccess: 'Projeto criado com sucesso!',
    updatedSuccess: 'Projeto atualizado com sucesso!',
    deactivatedSuccess: 'Projeto desativado com sucesso!',
    close: 'Fechar',
    empty: 'Nenhum projeto nesta categoria.',
    noDescription: 'Sem descrição'
  },
  en: {
    subtitle: 'Organize, track and manage your projects.',
    active: 'Active',
    inactive: 'Inactive',
    all: 'All',
    created: 'Created on',
    loading: 'Loading projects...',
    loadError: 'Could not load projects.',
    retry: 'Try again',
    edit: 'Edit',
    deactivate: 'Deactivate',
    confirmDeactivate: 'Deactivate project',
    confirmDescription: 'This project will become inactive. Continue?',
    cancel: 'Cancel',
    confirm: 'Confirm deactivation',
    createTitle: 'New project',
    editTitle: 'Edit project',
    name: 'Project name',
    description: 'Description',
    namePlaceholder: 'E.g. My project',
    descriptionPlaceholder: 'Describe the goal...',
    nameRequired: 'Enter a project name.',
    nameTooLong: 'Maximum 100 characters.',
    save: 'Save changes',
    create: 'Create project',
    saving: 'Saving...',
    saveError: 'Could not save the project.',
    deactivateError: 'Could not deactivate the project.',
    createdSuccess: 'Project created successfully!',
    updatedSuccess: 'Project updated successfully!',
    deactivatedSuccess: 'Project deactivated successfully!',
    close: 'Close',
    empty: 'No projects in this category.',
    noDescription: 'No description'
  },
  es: {
    subtitle: 'Organiza, supervisa y gestiona tus proyectos.',
    active: 'Activos',
    inactive: 'Inactivos',
    all: 'Todos',
    created: 'Creado el',
    loading: 'Cargando proyectos...',
    loadError: 'No se pudieron cargar los proyectos.',
    retry: 'Intentar de nuevo',
    edit: 'Editar',
    deactivate: 'Desactivar',
    confirmDeactivate: 'Desactivar proyecto',
    confirmDescription: 'Este proyecto quedará inactivo. ¿Continuar?',
    cancel: 'Cancelar',
    confirm: 'Confirmar desactivación',
    createTitle: 'Nuevo proyecto',
    editTitle: 'Editar proyecto',
    name: 'Nombre del proyecto',
    description: 'Descripción',
    namePlaceholder: 'Ej.: Mi proyecto',
    descriptionPlaceholder: 'Describe el objetivo...',
    nameRequired: 'Introduce el nombre del proyecto.',
    nameTooLong: 'Máximo 100 caracteres.',
    save: 'Guardar cambios',
    create: 'Crear proyecto',
    saving: 'Guardando...',
    saveError: 'No se pudo guardar el proyecto.',
    deactivateError: 'No se pudo desactivar el proyecto.',
    createdSuccess: '¡Proyecto creado correctamente!',
    updatedSuccess: '¡Proyecto actualizado correctamente!',
    deactivatedSuccess: '¡Proyecto desactivado correctamente!',
    close: 'Cerrar',
    empty: 'No hay proyectos en esta categoría.',
    noDescription: 'Sin descripción'
  }
};

type ProjectFilter = 'active' | 'inactive' | 'all';

@Component({
  selector: 'app-projects',
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './projects.html',
  styleUrl: './projects.scss'
})
export class Projects implements OnInit {
  readonly languageService = inject(LanguageService);
  readonly themeService = inject(ThemeService);

  private readonly authService = inject(AuthService);
  private readonly projectService = inject(ProjectService);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);

  readonly mobileMenuOpen = signal(false);
  readonly projects = signal<Project[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal(false);
  readonly filter = signal<ProjectFilter>('active');

  readonly modalOpen = signal(false);
  readonly editingId = signal<string | null>(null);
  readonly saving = signal(false);
  readonly saveError = signal(false);

  readonly confirmProject = signal<Project | null>(null);
  readonly deactivating = signal(false);
  readonly deactivateError = signal(false);

  readonly notice = signal<TextKey | null>(null);

  readonly projectForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['']
  });

  readonly activeProjects = computed(() =>
    this.projects().filter(project => project.isActive)
  );

  readonly inactiveProjects = computed(() =>
    this.projects().filter(project => !project.isActive)
  );

  readonly visibleProjects = computed(() => {
    const selected = this.filter();

    return [...this.projects()]
      .filter(project =>
        selected === 'all' ||
        (selected === 'active' && project.isActive) ||
        (selected === 'inactive' && !project.isActive)
      )
      .sort(
        (a, b) =>
          new Date(b.createdAt).getTime() -
          new Date(a.createdAt).getTime()
      );
  });

  ngOnInit(): void {
    this.loadProjects();
  }

  t(key: TextKey): string {
    return translations[this.languageService.language()][key];
  }

  loadProjects(): void {
    this.loading.set(true);
    this.loadError.set(false);

    this.projectService.getAll().subscribe({
      next: projects => {
        this.projects.set(projects);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        console.error('Erro ao carregar projetos:', error);
        this.loadError.set(true);
        this.loading.set(false);
      }
    });
  }

  setFilter(filter: ProjectFilter): void {
    this.filter.set(filter);
  }

  openCreate(): void {
    this.editingId.set(null);
    this.projectForm.reset();
    this.saveError.set(false);
    this.modalOpen.set(true);
  }

  openEdit(project: Project): void {
    if (!project.isActive) {
      return;
    }

    this.editingId.set(project.id);
    this.projectForm.setValue({
      name: project.name,
      description: project.description ?? ''
    });
    this.saveError.set(false);
    this.modalOpen.set(true);
  }

  closeModal(): void {
    if (this.saving()) {
      return;
    }

    this.modalOpen.set(false);
    this.editingId.set(null);
    this.saveError.set(false);
    this.projectForm.reset();
  }

  saveProject(): void {
    if (this.saving()) {
      return;
    }

    const name = this.projectForm.controls.name.value.trim();
    const description =
      this.projectForm.controls.description.value.trim();

    this.projectForm.controls.name.setValue(name);

    if (!name || name.length > 100 || this.projectForm.invalid) {
      this.projectForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.saveError.set(false);

    const id = this.editingId();

    const request = id
      ? this.projectService.update(id, { name, description })
      : this.projectService.create({ name, description });

    request.subscribe({
      next: project => {
        this.projects.update(current => [
          project,
          ...current.filter(item => item.id !== project.id)
        ]);

        this.saving.set(false);
        this.modalOpen.set(false);
        this.editingId.set(null);
        this.projectForm.reset();
        this.filter.set('active');
        this.notice.set(id ? 'updatedSuccess' : 'createdSuccess');
      },
      error: (error: HttpErrorResponse) => {
        console.error('Erro ao salvar projeto:', error);
        this.saving.set(false);
        this.saveError.set(true);
      }
    });
  }

  requestDeactivate(project: Project): void {
    if (!project.isActive) {
      return;
    }

    this.deactivateError.set(false);
    this.confirmProject.set(project);
  }

  closeConfirm(): void {
    if (this.deactivating()) {
      return;
    }

    this.confirmProject.set(null);
    this.deactivateError.set(false);
  }

  deactivateProject(): void {
    const project = this.confirmProject();

    if (!project || this.deactivating()) {
      return;
    }

    this.deactivating.set(true);
    this.deactivateError.set(false);

    this.projectService.deactivate(project.id).subscribe({
      next: () => {
        this.projects.update(current =>
          current.map(item =>
            item.id === project.id
              ? { ...item, isActive: false }
              : item
          )
        );

        this.deactivating.set(false);
        this.confirmProject.set(null);
        this.notice.set('deactivatedSuccess');
      },
      error: (error: HttpErrorResponse) => {
        console.error('Erro ao desativar projeto:', error);
        this.deactivating.set(false);
        this.deactivateError.set(true);
      }
    });
  }

  formatDate(date: string): string {
    const locale = {
      pt: 'pt-BR',
      en: 'en-US',
      es: 'es-ES'
    }[this.languageService.language()];

    const parsed = new Date(date);

    if (Number.isNaN(parsed.getTime())) {
      return '—';
    }

    return new Intl.DateTimeFormat(locale, {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).format(parsed);
  }

  changeLanguage(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.languageService.setLanguage(select.value as Language);
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update(open => !open);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  logout(): void {
    this.authService.logout();
    this.projects.set([]);
    void this.router.navigate(['/login']);
  }
}
