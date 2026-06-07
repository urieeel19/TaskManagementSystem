// src/app/features/tasks/tasks.routes.ts
import { Routes } from '@angular/router';
import { TaskDashboardComponent } from './pages/task-dashboard/task-dashboard.component';

export const TASKS_ROUTES: Routes = [
  {
    path: '',
    component: TaskDashboardComponent
  }
];