import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';

interface GoogleCredentialResponse {
  credential?: string;
}

interface GoogleAccountsId {
  initialize(options: {
    client_id: string;
    callback: (res: GoogleCredentialResponse) => void;
  }): void;
  prompt(): void;
  renderButton(parent: HTMLElement, options: Record<string, unknown>): void;
}

interface WindowWithGoogle extends Window {
  google?: {
    accounts?: {
      id?: GoogleAccountsId;
    };
  };
}

@Component({
  selector: 'app-auth-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './auth-modal.html',
})
export class AuthModalComponent implements OnInit {
  readonly authService = inject(AuthService);

  activeTab = signal<'login' | 'register'>('login');
  showGoogleConfig = signal<boolean>(false);

  // Google OAuth Configuration
  googleClientId = signal<string>('');

  // Form fields
  email = signal('pilot@skydeal.com');
  password = signal('Password123!');
  fullName = signal('');

  ngOnInit(): void {
    if (typeof window !== 'undefined') {
      const savedClientId = localStorage.getItem('skydeal-google-client-id') || '';
      this.googleClientId.set(savedClientId);

      if (savedClientId) {
        this.initGoogleClient(savedClientId);
      }
    }
  }

  setTab(tab: 'login' | 'register'): void {
    this.activeTab.set(tab);
    this.authService.errorMessage.set(null);
  }

  toggleGoogleConfig(): void {
    this.showGoogleConfig.update((v) => !v);
  }

  saveGoogleClientId(): void {
    const id = this.googleClientId().trim();
    if (id) {
      localStorage.setItem('skydeal-google-client-id', id);
      this.initGoogleClient(id);
      this.authService.errorMessage.set(null);
    }
  }

  onSubmit(): void {
    if (this.activeTab() === 'login') {
      if (!this.email().trim() || !this.password()) {
        this.authService.errorMessage.set('Inserisci sia email che password.');
        return;
      }

      this.authService
        .login({
          email: this.email().trim(),
          password: this.password(),
        })
        .subscribe();
    } else {
      if (!this.email().trim() || !this.password() || !this.fullName().trim()) {
        this.authService.errorMessage.set('Tutti i campi sono obbligatori per la registrazione.');
        return;
      }

      this.authService
        .register({
          email: this.email().trim(),
          password: this.password(),
          fullName: this.fullName().trim(),
        })
        .subscribe();
    }
  }

  onGoogleLogin(): void {
    const win = window as unknown as WindowWithGoogle;
    const clientId = this.googleClientId().trim();

    // Se è configurato un Client ID valido da Google Cloud, prova GIS
    if (clientId && win.google?.accounts?.id) {
      try {
        this.initGoogleClient(clientId);
        win.google.accounts.id.prompt();
        return;
      } catch (err) {
        console.warn('Errore prompt Google GIS, fallback su demo localhost:', err);
      }
    }

    // Modalità Localhost attiva: esegui login con account Google simulato
    // che invia la richiesta al backend C# (/api/auth/google) e genera il vero token JWT
    this.authService.loginWithGoogle('demo-google-token-localhost').subscribe();
  }

  private initGoogleClient(clientId: string): void {
    const win = window as unknown as WindowWithGoogle;
    if (win.google?.accounts?.id) {
      win.google.accounts.id.initialize({
        client_id: clientId,
        callback: (res: GoogleCredentialResponse) => {
          if (res.credential) {
            this.authService.loginWithGoogle(res.credential).subscribe();
          }
        },
      });
    }
  }
}
