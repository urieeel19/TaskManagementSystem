import { TodoStatus } from "../../enums/todo-status.enum";

export interface TaskUpdateResponseDto {
    id: string;         
    userId: string;     
    title: string;
    description: string;
    status: TodoStatus; 
    dueDate: string;    
  }