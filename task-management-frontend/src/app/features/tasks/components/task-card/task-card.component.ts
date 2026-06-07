import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TodoStatus } from 'src/app/core/enums/todo-status.enum';
import { TaskResponseDto } from 'src/app/core/models/getTask/task-response.dto';

@Component({
  selector: 'app-task-card',
  templateUrl: './task-card.component.html'
})
export class TaskCardComponent {
  @Input() task!: TaskResponseDto;

  @Output() edit = new EventEmitter<TaskResponseDto>();
  @Output() delete = new EventEmitter<string>();

  protected readonly TodoStatus = TodoStatus;
}