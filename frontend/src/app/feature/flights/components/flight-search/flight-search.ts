import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SkyDealRadarAPIService } from '../../../../core/api/endpoints.service';
import { LowCostFlightFlightsSearchFlightsFlightDealItem } from '../../../../core/api/models';

@Component({
  selector: 'app-flight-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './flight-search.html',
})
export class FlightSearchComponent {
  private readonly flightService = inject(SkyDealRadarAPIService);

  // Filtri di ricerca API
  departureId = signal('MXP');
  arrivalId = signal('');
  currency = signal('EUR');
  outboundDate = signal('');
  returnDate = signal('');
  maxPrice = signal<number | null>(null);
  stops = signal<number>(0);

  // Filtri e ordinamento client-side (Shadcn style tab)
  activeTab = signal<'all' | 'under50' | 'direct' | 'topDiscount'>('all');
  sortBy = signal<'price' | 'discount' | 'duration'>('price');

  // Stati reattivi UI
  flights = signal<LowCostFlightFlightsSearchFlightsFlightDealItem[]>([]);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  hasSearched = signal(false);

  // Destinazioni rapide d'ispirazione
  readonly popularAirports = [
    { code: '', label: 'Tutte le mete', icon: '🌍' },
    { code: 'BCN', label: 'Barcellona', icon: '🏖️' },
    { code: 'LHR', label: 'Londra', icon: '🎡' },
    { code: 'CDG', label: 'Parigi', icon: '🗼' },
    { code: 'AMS', label: 'Amsterdam', icon: '🚲' },
    { code: 'JFK', label: 'New York', icon: '🗽' },
  ];

  // Voli filtrati e ordinati reattivamente (solo voli con sconto reale)
  readonly visibleFlights = computed(() => {
    let list = this.flights().filter((f) => (f.discountPercentage ?? 0) > 0);

    // Filtro rapido tab
    if (this.activeTab() === 'under50') {
      list = list.filter((f) => (f.price ?? 0) <= 50);
    } else if (this.activeTab() === 'direct') {
      list = list.filter((f) => f.stops === 0);
    } else if (this.activeTab() === 'topDiscount') {
      list = list.filter((f) => (f.discountPercentage ?? 0) >= 30);
    }

    // Ordinamento
    if (this.sortBy() === 'price') {
      list.sort((a, b) => (a.price ?? 0) - (b.price ?? 0));
    } else if (this.sortBy() === 'discount') {
      list.sort((a, b) => (b.discountPercentage ?? 0) - (a.discountPercentage ?? 0));
    } else if (this.sortBy() === 'duration') {
      list.sort((a, b) => (a.durationInMinutes ?? 0) - (b.durationInMinutes ?? 0));
    }

    return list;
  });

  selectPopularAirport(code: string): void {
    this.arrivalId.set(code);
    this.onSearch();
  }

  swapAirports(): void {
    const currentDep = this.departureId();
    const currentArr = this.arrivalId();
    if (currentArr) {
      this.departureId.set(currentArr);
      this.arrivalId.set(currentDep);
      this.onSearch();
    }
  }

  resetFilters(): void {
    this.arrivalId.set('');
    this.outboundDate.set('');
    this.returnDate.set('');
    this.maxPrice.set(null);
    this.stops.set(0);
    this.activeTab.set('all');
    this.sortBy.set('price');
    this.onSearch();
  }

  setFilterTab(tab: 'all' | 'under50' | 'direct' | 'topDiscount'): void {
    this.activeTab.set(tab);
  }

  setSortBy(sort: 'price' | 'discount' | 'duration'): void {
    this.sortBy.set(sort);
  }

  onSearch(): void {
    if (!this.departureId().trim()) {
      this.errorMessage.set('Inserisci almeno un aeroporto di partenza (es. MXP, FCO).');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.hasSearched.set(true);

    this.flightService
      .lowCostFlightFlightsSearchFlightsSearchFlightsEndpoint({
        departureId: this.departureId().trim().toUpperCase(),
        arrivalId: this.arrivalId().trim() ? this.arrivalId().trim().toUpperCase() : null,
        currency: this.currency(),
        outboundDate: this.outboundDate() || null,
        returnDate: this.returnDate() || null,
        maxPrice: this.maxPrice(),
        stops: this.stops() !== 0 ? this.stops() : null,
      })
      .subscribe({
        next: (response) => {
          this.flights.set(response.deals ?? []);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Errore chiamata voli:', err);
          const detail =
            err?.error?.message || err?.message || 'Errore nel recupero delle offerte voli.';
          this.errorMessage.set(detail);
          this.flights.set([]);
          this.isLoading.set(false);
        },
      });
  }

  formatDuration(minutes: number | undefined): string {
    if (!minutes) return 'N/D';
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return h > 0 ? `${h}h ${m.toString().padStart(2, '0')}m` : `${m}m`;
  }
}
