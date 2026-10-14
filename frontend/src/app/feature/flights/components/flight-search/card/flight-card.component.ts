import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideCalendar, LucideExternalLink } from '@lucide/angular';
import { LowCostFlightFlightsSearchFlightsFlightDealItem } from '../../../../../core/api/models';

@Component({
  selector: 'app-flight-card',
  standalone: true,
  imports: [CommonModule, LucideCalendar, LucideExternalLink],
  templateUrl: './flight-card.component.html',
})
export class FlightCardComponent {
  // Riceve il singolo volo dal genitore tramite Signal input
  readonly flight = input.required<LowCostFlightFlightsSearchFlightsFlightDealItem>();

  formatDuration(minutes?: number | null): string {
    if (!minutes) return 'N/D';
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return `${h}h ${m}m`;
  }
}
