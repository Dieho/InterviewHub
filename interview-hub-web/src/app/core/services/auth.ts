// core/services/auth.ts
import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

interface AuthResponse {
  token: string;
  email: string;
  role: string;
}

interface LoginRequest {
  email: string;
  password: string;
}

interface JwtPayload {
  sub: string;
  email: string;
  role: string;
  exp: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = 'http://localhost:5118/auth';
  private readonly tokenKey = 'auth_token';

  currentToken = signal<string | null>(localStorage.getItem(this.tokenKey));

  // computed — реактивне значення, що ПЕРЕРАХОВУЄТЬСЯ автоматично, коли currentToken змінюється
  currentEmail = computed(() => this.decodeToken()?.email ?? null);
  currentRole = computed(() => this.decodeToken()?.role ?? null);
  isAdmin = computed(() => this.currentRole() === 'Admin');

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/login`, { email, password } as LoginRequest)
      .pipe(tap((response) => this.storeToken(response.token)));
  }

  register(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/register`, { email, password } as LoginRequest)
      .pipe(tap((response) => this.storeToken(response.token)));
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.currentToken.set(null);
  }

  isLoggedIn(): boolean {
    return this.currentToken() !== null;
  }

  private storeToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
    this.currentToken.set(token);
  }

  private decodeToken(): JwtPayload | null {
    const token = this.currentToken();
    if (!token) return null;

    try {
      const payloadBase64 = token.split('.')[1];        // друга частина: header.PAYLOAD.signature
      const decoded = atob(payloadBase64);                // atob — вбудована браузерна Base64-декодування
      return JSON.parse(decoded) as JwtPayload;
    } catch {
      return null;
    }
  }
}