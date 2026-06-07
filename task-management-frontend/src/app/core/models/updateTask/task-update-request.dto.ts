import { TodoStatus } from "../../enums/todo-status.enum";

export interface TaskUpdateRequestDto {
  id: string;        
  title: string;
  description: string;
  status: TodoStatus; 
  dueDate: string;    
}