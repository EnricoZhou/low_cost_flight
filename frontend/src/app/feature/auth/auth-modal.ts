import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-auth-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './auth-modal.html',
})
export class AuthModalComponent {
  readonly authService = inject(AuthService);

  activeTab = signal<'login' | 'register'>('login');

  // Form fields
  email = signal('');
  password = signal('');
  fullName = signal('');

  setTab(tab: 'login' | 'register'): void {
    this.activeTab.set(tab);
    this.authService.errorMessage.set(null);
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
    this.authService.loginWithGoogle('demo-google-token-localhost').subscribe();
  }
}
