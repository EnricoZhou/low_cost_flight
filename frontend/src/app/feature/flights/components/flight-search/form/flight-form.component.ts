import { Component, ChangeDetectionStrategy, input, model, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  LucideCalendar,
  LucidePlane,
  LucideMapPin,
  LucideArrowLeftRight,
  LucideX,
  LucideSearch,
} from '@lucide/angular';
import { FlightSearchParams } from '../../../models';

@Component({
  selector: 'app-flight-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LucideCalendar,
    LucidePlane,
    LucideMapPin,
    LucideArrowLeftRight,
    LucideX,
    LucideSearch,
  ],
  templateUrl: './flight-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FlightFormComponent {
  readonly isLoading = input<boolean>(false);
  readonly filters = model.required<FlightSearchParams>();
  readonly search = output<void>();

  // Destinazioni rapide d'ispirazione
  readonly popularAirports = [
    { code: '', label: 'Tutte le mete', icon: '🌍' },
    { code: 'BCN', label: 'Barcellona', icon: '🏖️' },
    { code: 'LHR', label: 'Londra', icon: '🎡' },
    { code: 'CDG', label: 'Parigi', icon: '🗼' },
    { code: 'AMS', label: 'Amsterdam', icon: '🚲' },
    { code: 'JFK', label: 'New York', icon: '🗽' },
  ];

  updateField<K extends keyof FlightSearchParams>(field: K, value: FlightSearchParams[K]): void {
    this.filters.update((prev) => ({ ...prev, [field]: value }));
  }

  selectPopularAirport(code: string): void {
    this.updateField('arrivalId', code);
    this.onSearch();
  }

  swapAirports(): void {
    const currentDep = this.filters().departureId;
    const currentArr = this.filters().arrivalId;
    if (currentArr) {
      this.filters.update((prev) => ({
        ...prev,
        departureId: currentArr,
        arrivalId: currentDep,
      }));
      this.onSearch();
    }
  }

  onSearch(): void {
    if (!this.filters().departureId.trim()) {
      return;
    }
    this.search.emit();
  }
}
