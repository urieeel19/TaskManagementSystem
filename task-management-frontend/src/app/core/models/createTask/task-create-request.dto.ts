import { TodoStatus } from "../../enums/todo-status.enum";

export interface TaskCreateRequestDto {
    title: string;
    description: string;
    status: TodoStatus; 
    dueDate: string;  
  }