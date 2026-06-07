import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { TaskCreateRequestDto } from '../models/createTask/task-create-request.dto';
import { TaskCreateResponseDto } from '../models/createTask/task-create-response.dto';
import { TaskUpdateRequestDto } from '../models/updateTask/task-update-request.dto';
import { TaskUpdateResponseDto } from '../models/updateTask/task-update-response.dto';
import { TaskResponseDto } from '../models/getTask/task-response.dto';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private readonly apiUrl = `${environment.apiUrl}/tasks`; 
  private tasksSubject = new BehaviorSubject<TaskResponseDto[]>([]);
  public tasks$ = this.tasksSubject.asObservable();

  constructor(private http: HttpClient) {}

  /**
   * READ (Get All): Retrieves all tasks assigned to the authenticated user
   */
  loadAll(): void {
    this.http.get<TaskResponseDto[]>(this.apiUrl)
      .pipe(
        catchError((error: HttpErrorResponse) => this.handleError('Critical error retrieving the task list from the API', error))
      )
      .subscribe({
        next: (tasks: TaskResponseDto[]) => {
          const sortedTasks = tasks.sort((a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime());
          this.tasksSubject.next(sortedTasks);
        }
      });
  }

  /**
   * CREATE (Post): Submits the creation payload and refreshes the reactive stream
   * @param request Payload without ID or UserId
   */
  create(request: TaskCreateRequestDto): Observable<TaskCreateResponseDto> {
    return this.http.post<TaskCreateResponseDto>(this.apiUrl, request).pipe(
      tap(() => this.loadAll()),
      catchError((error: HttpErrorResponse) => this.handleError('Failed to create the task', error))
    );
  }

  /**
   * UPDATE (Put): Submits the updated payload targeting an existing resource GUID
   * @param request Payload including the mandatory ID in the request body
   */
  update(request: TaskUpdateRequestDto): Observable<TaskUpdateResponseDto> {
    return this.http.put<TaskUpdateResponseDto>(`${this.apiUrl}/${request.id}`, request).pipe(
      tap(() => this.loadAll()),
      catchError((error: HttpErrorResponse) => this.handleError('Failed to update the task', error))
    );
  }

  /**
   * DELETE (Delete): Removes the physical database record using its unique GUID
   * @param id Unique task GUID to be removed
   */
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        const currentTasks = this.tasksSubject.value;
        const updatedTasks = currentTasks.filter(task => task.id !== id);
        this.tasksSubject.next(updatedTasks);
      }),
      catchError((error: HttpErrorResponse) => this.handleError('Failed to delete the task', error))
    );
  }

  /**
   * Centralized HTTP Error Handler to guarantee error transparency across the app
   */
  private handleError(contextMessage: string, error: HttpErrorResponse): Observable<never> {
    console.error('Status Code:', error.status);
    console.error('Backend Response Body:', error.error);
        return throwError(() => error);
  }
}