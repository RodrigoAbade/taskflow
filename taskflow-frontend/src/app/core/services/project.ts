
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Project } from '../../shared/models/project.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = '/api/projects';

  getAll(): Observable<Project[]> {
    return this.http.get<Project[]>(this.apiUrl);
  }

  getById(id: string): Observable<Project> {
    return this.http.get<Project>(
      `${this.apiUrl}/${id}`
    );
  }

  create(data: {
    name: string;
    description: string;
  }): Observable<Project> {
    return this.http.post<Project>(
      this.apiUrl,
      data
    );
  }

  update(
    id: string,
    data: {
      name: string;
      description: string;
    }
  ): Observable<Project> {
    return this.http.put<Project>(
      `${this.apiUrl}/${id}`,
      data
    );
  }

  deactivate(id: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${id}`
    );
  }
}
