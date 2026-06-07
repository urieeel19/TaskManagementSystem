import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TaskService } from '../../../../core/services/task.service';
import { AuthService } from '../../../../core/services/auth.service';
import { TaskCreateRequestDto } from 'src/app/core/models/createTask/task-create-request.dto';
import { TaskResponseDto } from 'src/app/core/models/getTask/task-response.dto';
import { TaskUpdateRequestDto } from 'src/app/core/models/updateTask/task-update-request.dto';

@Component({
  selector: 'app-task-dashboard',
  templateUrl: './task-dashboard.component.html'
})
export class TaskDashboardComponent implements OnInit {
  tasks$ = this.taskService.tasks$;
  isModalOpen = false;
  selectedTask: TaskResponseDto | null = null;
  apiTaskError: string | null = null;
  apiDashboardError: string | null = null; 

  constructor(
    public taskService: TaskService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.taskService.loadAll();
  }

  openCreateModal(): void {
    this.apiTaskError = null;
    this.selectedTask = null;
    this.isModalOpen = true;
  }

  openEditModal(task: TaskResponseDto): void {
    this.apiTaskError = null;
    this.selectedTask = task; 
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.selectedTask = null;
    this.apiTaskError = null;
  }

  handleCreate(payload: TaskCreateRequestDto): void {
    this.taskService.create(payload).subscribe({
      next: () => this.closeModal(),
      error: (err) => {
        this.apiTaskError = err.error?.message || err.error?.error || 'Failed to create task. Please try again.';
      }
    });
  }

  handleUpdate(payload: TaskUpdateRequestDto): void {
    this.taskService.update(payload).subscribe({
      next: () => this.closeModal(),
      error: (err) => {
        this.apiTaskError = err.error?.message || err.error?.error || 'Failed to update task. Please try again.';
      }
    });
  }

  onDeleteTask(id: string): void {
    this.apiDashboardError = null;
    if (confirm('Are you sure you want to permanently delete this task?')) {
      this.taskService.delete(id).subscribe({
        error: (err) => {
          this.apiDashboardError = err.error?.message || err.error?.error || 'Failed to delete task. Please try again.';
        }
      });
    }
  }

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  trackByTaskId(index: number, item: TaskResponseDto): string {
    return item.id;
  }
}