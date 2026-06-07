export interface AuthRequestDto {
    username: string;
    password: string;
  }
  
  export interface AuthResponseDto {
    token: string;
    username: string;
  }
  export interface UserRegisterDto {
    username: string;
    password: string;
    confirmPassword: string;
  }