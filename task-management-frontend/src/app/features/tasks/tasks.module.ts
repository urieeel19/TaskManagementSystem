import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TaskDashboardComponent } from './pages/task-dashboard/task-dashboard.component';
import { TaskCardComponent } from './components/task-card/task-card.component';
import { TaskModalFormComponent } from './components/task-modal-form/task-modal-form.component';
import { TASKS_ROUTES } from './tasks.routes';
import { SharedModule } from 'src/app/shared/shared.module';

@NgModule({
  declarations: [
    TaskDashboardComponent,
    TaskCardComponent,
    TaskModalFormComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule, 
    RouterModule.forChild(TASKS_ROUTES),
    SharedModule
  ]
})
export class TasksModule { }