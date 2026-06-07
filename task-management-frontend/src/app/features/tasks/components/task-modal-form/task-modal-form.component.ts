import { Component, EventEmitter, Input, OnInit, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TodoStatus } from 'src/app/core/enums/todo-status.enum';
import { TaskCreateRequestDto } from 'src/app/core/models/createTask/task-create-request.dto';
import { TaskResponseDto } from 'src/app/core/models/getTask/task-response.dto';
import { TaskUpdateRequestDto } from 'src/app/core/models/updateTask/task-update-request.dto';

@Component({
  selector: 'app-task-modal-form',
  templateUrl: './task-modal-form.component.html'
})
export class TaskModalFormComponent implements OnInit, OnChanges {
  @Input() taskToEdit: TaskResponseDto | null = null;
  @Input() errorMessage: string | null = null; 

  @Output() saveCreate = new EventEmitter<TaskCreateRequestDto>();
  @Output() saveUpdate = new EventEmitter<TaskUpdateRequestDto>();
  @Output() close = new EventEmitter<void>();
  @Output() clearError = new EventEmitter<void>();

  taskForm!: FormGroup;
  isEditMode = false;

  statusOptions = [
    { value: TodoStatus.Pending.toString(), label: 'Pending' },
    { value: TodoStatus.InProgress.toString(), label: 'In Progress' },
    { value: TodoStatus.Completed.toString(), label: 'Completed' }
  ];

  constructor(private fb: FormBuilder) {
    this.initForm();
  }

  ngOnInit(): void {
    this.updateFormState();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['taskToEdit'] && this.taskForm) {
      this.updateFormState();
    }
  }

  private updateFormState(): void {
    this.isEditMode = !!this.taskToEdit;
    
    let formattedDate = '';
    if (this.taskToEdit?.dueDate) {
      formattedDate = new Date(this.taskToEdit.dueDate).toISOString().split('T')[0];
    }

    this.taskForm.patchValue({
      title: this.taskToEdit?.title || '',
      description: this.taskToEdit?.description || '',
      status: this.taskToEdit?.status !== undefined && this.taskToEdit?.status !== null 
        ? this.taskToEdit.status.toString() 
        : TodoStatus.Pending.toString(),
      dueDate: formattedDate
    });
  }

  private initForm(): void {
    this.taskForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      status: [TodoStatus.Pending.toString(), [Validators.required]],
      dueDate: ['', [Validators.required]]
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.taskForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  onDismissError(): void {
    this.errorMessage = null;
    this.clearError.emit();
  }

  onSubmit(): void {
    if (this.taskForm.invalid) {
      this.taskForm.markAllAsTouched();
      return;
    }

    this.onDismissError();

    const formValue = this.taskForm.value;
    const isoDueDate = new Date(formValue.dueDate).toISOString();
    const numericStatus = Number(formValue.status);

    if (this.isEditMode && this.taskToEdit) {
      const updatePayload: TaskUpdateRequestDto = {
        id: this.taskToEdit.id,
        title: formValue.title.trim(),
        description: formValue.description?.trim() || '',
        status: numericStatus,
        dueDate: isoDueDate
      };
      this.saveUpdate.emit(updatePayload);
    } else {
      const createPayload: TaskCreateRequestDto = {
        title: formValue.title.trim(),
        description: formValue.description?.trim() || '',
        status: numericStatus,
        dueDate: isoDueDate
      };
      this.saveCreate.emit(createPayload);
    }
  }
}