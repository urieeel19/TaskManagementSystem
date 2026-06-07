import { Routes } from '@angular/router';
import { AuthLayoutComponent } from './pages/auth-layout/auth-layout.component';
import { LoginFormComponent } from './components/login-form/login-form.component';
import { RegisterFormComponent } from './components/register-form/register-form.component';

export const AUTH_ROUTES: Routes = [
  {
    path: '',
    component: AuthLayoutComponent,
    children: [
      { 
        path: '', 
        redirectTo: 'login', 
        pathMatch: 'full' 
      },
      { 
        path: 'login', 
        component: LoginFormComponent 
      },
      { 
        path: 'register', 
        component: RegisterFormComponent 
      }
    ]
  }
];