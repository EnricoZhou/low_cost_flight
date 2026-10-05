import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FlightSearchComponent } from './feature/flights/components/flight-search/flight-search';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FlightSearchComponent],
  templateUrl: './app.html',
})
export class App {
  readonly themeService = inject(ThemeService);
}
