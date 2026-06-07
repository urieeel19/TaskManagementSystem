import { TodoStatus } from "../../enums/todo-status.enum";

export interface TaskCreateResponseDto {
    id: string;        
    userId: string;    
    title: string;
    description: string;
    status: TodoStatus; 
    dueDate: string;    
  }