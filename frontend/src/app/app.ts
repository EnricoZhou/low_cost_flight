import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FlightSearchComponent } from './feature/flights/components/flight-search/flight-search';
import { AuthModalComponent } from './feature/auth/auth-modal';
import { ThemeService } from './core/services/theme.service';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FlightSearchComponent, AuthModalComponent],
  templateUrl: './app.html',
})
export class App {
  readonly themeService = inject(ThemeService);
  readonly authService = inject(AuthService);
}
