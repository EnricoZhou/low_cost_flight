import { Injectable, computed, inject, signal } from '@angular/core';
import { SkyDealRadarAPIService } from '../api/endpoints.service';
import {
  LowCostFlightAuthCommonAuthResponse,
  LowCostFlightAuthCommonUserDto,
  LowCostFlightAuthLoginLoginRequest,
  LowCostFlightAuthRegisterRegisterRequest,
} from '../api/models';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly api = inject(SkyDealRadarAPIService);
  private readonly tokenStorageKey = 'skydeal-jwt-token';
  private readonly userStorageKey = 'skydeal-user';

  // Signals di stato
  readonly currentUser = signal<LowCostFlightAuthCommonUserDto | null>(null);
  readonly token = signal<string | null>(null);
  readonly isAuthModalOpen = signal<boolean>(false);
  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  // Computed state
  readonly isAuthenticated = computed(() => !!this.token() && !!this.currentUser());

  constructor() {
    this.initFromStorage();
  }

  openAuthModal(): void {
    this.errorMessage.set(null);
    this.isAuthModalOpen.set(true);
  }

  closeAuthModal(): void {
    this.isAuthModalOpen.set(false);
  }

  login(req: LowCostFlightAuthLoginLoginRequest): Observable<LowCostFlightAuthCommonAuthResponse> {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    return this.api.lowCostFlightAuthLoginLoginEndpoint(req).pipe(
      tap({
        next: (res) => {
          this.handleAuthSuccess(res);
          this.isLoading.set(false);
          this.closeAuthModal();
        },
        error: (err) => {
          this.isLoading.set(false);
          const detail =
            err?.error?.message ||
            err?.error?.errors?.[0]?.message ||
            'Credenziali errate o errore durante il login.';
          this.errorMessage.set(detail);
        },
      }),
    );
  }

  register(
    req: LowCostFlightAuthRegisterRegisterRequest,
  ): Observable<LowCostFlightAuthCommonAuthResponse> {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    return this.api.lowCostFlightAuthRegisterRegisterEndpoint(req).pipe(
      tap({
        next: (res) => {
          this.handleAuthSuccess(res);
          this.isLoading.set(false);
          this.closeAuthModal();
        },
        error: (err) => {
          this.isLoading.set(false);
          const detail =
            err?.error?.message ||
            err?.error?.errors?.[0]?.message ||
            'Errore durante la registrazione.';
          this.errorMessage.set(detail);
        },
      }),
    );
  }

  loginWithGoogle(idToken: string): Observable<LowCostFlightAuthCommonAuthResponse> {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    return this.api.lowCostFlightAuthGoogleLoginGoogleLoginEndpoint({ idToken }).pipe(
      tap({
        next: (res) => {
          this.handleAuthSuccess(res);
          this.isLoading.set(false);
          this.closeAuthModal();
        },
        error: (err) => {
          this.isLoading.set(false);
          const detail =
            err?.error?.message ||
            err?.error?.errors?.[0]?.message ||
            'Autenticazione Google fallita.';
          this.errorMessage.set(detail);
        },
      }),
    );
  }

  logout(): void {
    this.token.set(null);
    this.currentUser.set(null);
    if (typeof window !== 'undefined') {
      localStorage.removeItem(this.tokenStorageKey);
      localStorage.removeItem(this.userStorageKey);
    }
  }

  private handleAuthSuccess(response: LowCostFlightAuthCommonAuthResponse): void {
    if (response.token && response.user) {
      this.token.set(response.token);
      this.currentUser.set(response.user);

      if (typeof window !== 'undefined') {
        localStorage.setItem(this.tokenStorageKey, response.token);
        localStorage.setItem(this.userStorageKey, JSON.stringify(response.user));
      }
    }
  }

  private initFromStorage(): void {
    if (typeof window === 'undefined') return;

    const savedToken = localStorage.getItem(this.tokenStorageKey);
    const savedUserJson = localStorage.getItem(this.userStorageKey);

    if (savedToken && savedUserJson) {
      try {
        const user = JSON.parse(savedUserJson) as LowCostFlightAuthCommonUserDto;
        this.token.set(savedToken);
        this.currentUser.set(user);
      } catch (e) {
        console.error('Errore parsing dati utente da storage:', e);
        this.logout();
      }
    }
  }
}
