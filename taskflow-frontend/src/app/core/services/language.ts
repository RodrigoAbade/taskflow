
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
  | 'theme'
  | 'dashboard'
  | 'overview'
  | 'projects'
  | 'tasks'
  | 'inProgress'
  | 'completed'
  | 'recentProjects'
  | 'noProjects'
  | 'dashboardSubtitle'
  | 'totalProjects'
  | 'totalTasks'
  | 'logout'
  | 'settings'
  | 'lightTheme'
  | 'darkTheme'
  | 'newProject'
  | 'viewAll'
  | 'navigation'
  | 'workspace';

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
    theme: 'Tema',

    dashboard: 'Dashboard',
    overview: 'Visão geral',
    projects: 'Projetos',
    tasks: 'Tarefas',
    inProgress: 'Em andamento',
    completed: 'Concluídas',
    recentProjects: 'Projetos recentes',
    noProjects: 'Nenhum projeto encontrado.',
    dashboardSubtitle: 'Acompanhe seus projetos e tarefas em um só lugar.',
    totalProjects: 'Total de projetos',
    totalTasks: 'Total de tarefas',
    logout: 'Sair da conta',
    settings: 'Configurações',
    lightTheme: 'Tema claro',
    darkTheme: 'Tema escuro',
    newProject: 'Novo projeto',
    viewAll: 'Ver todos',
    navigation: 'Navegação',
    workspace: 'Área de trabalho'
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
    theme: 'Theme',

    dashboard: 'Dashboard',
    overview: 'Overview',
    projects: 'Projects',
    tasks: 'Tasks',
    inProgress: 'In progress',
    completed: 'Completed',
    recentProjects: 'Recent projects',
    noProjects: 'No projects found.',
    dashboardSubtitle: 'Track your projects and tasks in one place.',
    totalProjects: 'Total projects',
    totalTasks: 'Total tasks',
    logout: 'Sign out',
    settings: 'Settings',
    lightTheme: 'Light theme',
    darkTheme: 'Dark theme',
    newProject: 'New project',
    viewAll: 'View all',
    navigation: 'Navigation',
    workspace: 'Workspace'
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
    theme: 'Tema',

    dashboard: 'Panel',
    overview: 'Resumen',
    projects: 'Proyectos',
    tasks: 'Tareas',
    inProgress: 'En progreso',
    completed: 'Completadas',
    recentProjects: 'Proyectos recientes',
    noProjects: 'No se encontraron proyectos.',
    dashboardSubtitle: 'Gestiona tus proyectos y tareas en un solo lugar.',
    totalProjects: 'Total de proyectos',
    totalTasks: 'Total de tareas',
    logout: 'Cerrar sesión',
    settings: 'Configuración',
    lightTheme: 'Tema claro',
    darkTheme: 'Tema oscuro',
    newProject: 'Nuevo proyecto',
    viewAll: 'Ver todos',
    navigation: 'Navegación',
    workspace: 'Espacio de trabajo'
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
