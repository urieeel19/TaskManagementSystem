// src/app/core/services/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthRequestDto, AuthResponseDto } from '../models/auth.dto';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`; 
  private tokenSubject = new BehaviorSubject<string | null>(localStorage.getItem('token'));
  
  public token$ = this.tokenSubject.asObservable();
  
  public isAuthenticated$ = this.token$.pipe(
    map(token => this.isTokenValid(token))
  );

  constructor(private http: HttpClient) {}

  /**
   * Registers a new user account in the .NET identity database
   * @param request User credentials payload
   */
  register(request: AuthRequestDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/register`, request).pipe(
      catchError((error: HttpErrorResponse) => this.handleError('Registration request failed', error))
    );
  }

  /**
   * Authenticates user credentials and establishes a persistent secure session
   * @param request User credentials payload
   */
  login(request: AuthRequestDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.apiUrl}/login`, request).pipe(
      tap((response: AuthResponseDto) => {
        // Enforce synchronization between state storage and reactive memory
        localStorage.setItem('token', response.token);
        this.tokenSubject.next(response.token);
      }),
      catchError((error: HttpErrorResponse) => this.handleError('Authentication workflow failed', error))
    );
  }

  /**
   * Terminates the current session and clears security tokens from all layers
   */
  logout(): void {
    localStorage.removeItem('token');
    this.tokenSubject.next(null);
  }

  isAuthenticated(): boolean {
    return this.isTokenValid(this.getToken());
  }

  /**
   * Retrieves the raw JWT string token from the active application state
   */
  getToken(): string | null {
    return this.tokenSubject.value;
  }

  /**
   * 🌟 ADDED: Decodes JWT payload natively via 'atob' and evaluates lifetime constraints against Unix time
   * @param token Raw JWT base64 string
   */
  private isTokenValid(token: string | null): boolean {
    if (!token) return false;

    try {
      const parts = token.split('.');
      if (parts.length !== 3) return false;

      const payloadDecoded = window.atob(parts[1]);
      const payloadData = JSON.parse(payloadDecoded);

      if (!payloadData.exp) return true;

      const currentTime = Math.floor(Date.now() / 1000);
      
      if (payloadData.exp < currentTime) {
        console.warn('[AuthService] Active JWT token has expired. Forcing local logout.');
        this.logout();
        return false;
      }

      return true;
    } catch (error) {
      console.error('[AuthService] Failed to parse or decode standard token schema:', error);
      this.logout();
      return false;
    }
  }

  /**
   * Centralized HTTP Error Handler to provide logs transparency for debugging
   */
  private handleError(contextMessage: string, error: HttpErrorResponse): Observable<never> {
    console.error(`[AuthService Error] ${contextMessage}`);
    console.error('Status Code:', error.status);
    console.error('Server Message Content:', error.error);
    
    return throwError(() => error);
  }
}