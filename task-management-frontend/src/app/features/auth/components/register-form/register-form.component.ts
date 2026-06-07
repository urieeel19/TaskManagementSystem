// src/app/features/auth/components/register-form/register-form.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { UserRegisterDto } from '../../../../core/models/auth.dto';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-register-form',
  templateUrl: './register-form.component.html',
  styleUrls: ['./register-form.component.scss']
})
export class RegisterFormComponent implements OnInit, OnDestroy {
  registerForm!: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  
  private destroy$ = new Subject<void>();
  private redirectTimeoutId: any = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initForm();
  }

  /**
   * Defensive cleanup during component destruction phase
   */
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();

    if (this.redirectTimeoutId) {
      clearTimeout(this.redirectTimeoutId);
    }

    // Restores pointer control to the document root safely
    document.body.style.pointerEvents = 'auto';
  }

  /**
   * Initializes the reactive form structure matching strictly the 3-field backend DTO
   */
  private initForm(): void {
    this.registerForm = this.fb.group({
      username: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(50)
      ]],
      password: ['', [
        Validators.required,
        Validators.minLength(6)
      ]],
      confirmPassword: ['', [
        Validators.required
      ]]
    }, { 
      validators: this.passwordMatchValidator // Cross-field validator for matching passwords
    });
  }

  /**
   * Custom validator to ensure confirmation field strictly mirrors the original password
   */
  private passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    return null;
  }

  /**
   * Semantic helper to evaluate UI error display states per form control
   */
  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  /**
   * Securely submits the payload to the registration endpoint
   */
  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    // Removes active interaction focus from the submit button before transitions
    if (document.activeElement instanceof HTMLElement) {
      document.activeElement.blur();
    }

    this.isLoading = true;
    this.errorMessage = null;
    this.successMessage = null;

    const payload: UserRegisterDto = {
      username: this.registerForm.value.username.trim(),
      password: this.registerForm.value.password,
      confirmPassword: this.registerForm.value.confirmPassword
    };

    this.authService.register(payload)
      .pipe(takeUntil(this.destroy$)) 
      .subscribe({
        next: () => {
          this.isLoading = false;
          this.successMessage = 'Account created successfully! Redirecting to login...';
          
          document.body.style.pointerEvents = 'auto';

          this.redirectTimeoutId = setTimeout(() => {
            this.router.navigate(['/login']).then(() => {
              window.scrollTo({ top: 0 });
            });
          }, 2000);
        },
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = err.error?.message || err.error?.error || 'Registration failed. Please check your inputs.';
        }
      });
  }
}