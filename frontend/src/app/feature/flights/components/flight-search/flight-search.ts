import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SkyDealRadarAPIService } from '../../../../core/api/endpoints.service';
import { LowCostFlightFlightsSearchFlightsFlightDealItem } from '../../../../core/api/models';
import { FlightCardComponent } from './card/flight-card.component';
import { FlightHeroComponent } from './hero/flight-hero.component';
import { FlightFormComponent } from './form/flight-form.component';
import { FlightSearchParams, DEFAULT_SEARCH_PARAMS } from '../../models';


@Component({
  selector: 'app-flight-search',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    FlightCardComponent,
    FlightHeroComponent,
    FlightFormComponent,
  ],
  templateUrl: './flight-search.html',
})
export class FlightSearchComponent {
  private readonly flightService = inject(SkyDealRadarAPIService);

  // Filtri di ricerca API (stato centralizzato nel componente genitore)
  filters = signal<FlightSearchParams>({ ...DEFAULT_SEARCH_PARAMS });

  // Filtri e ordinamento client-side (Shadcn style tab)
  activeTab = signal<'all' | 'under50' | 'direct' | 'topDiscount'>('all');
  sortBy = signal<'price' | 'discount' | 'duration'>('price');

  // Stati reattivi UI
  flights = signal<LowCostFlightFlightsSearchFlightsFlightDealItem[]>([]);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  hasSearched = signal(false);

  // Voli filtrati e ordinati reattivamente (solo voli con sconto reale o destinazione specifica)
  readonly visibleFlights = computed(() => {
    const hasArrival = !!this.filters().arrivalId?.trim();
    let list = this.flights().filter((f) => (f.discountPercentage ?? 0) > 0 || hasArrival);

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

  resetFilters(): void {
    this.filters.set({ ...DEFAULT_SEARCH_PARAMS });
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
    const params = this.filters();
    if (!params.departureId?.trim()) {
      this.errorMessage.set('Inserisci almeno un aeroporto di partenza (es. MXP, FCO).');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.hasSearched.set(true);

    this.flightService
      .lowCostFlightFlightsSearchFlightsSearchFlightsEndpoint({
        departureId: params.departureId.trim().toUpperCase(),
        arrivalId: params.arrivalId?.trim() ? params.arrivalId.trim().toUpperCase() : null,
        currency: params.currency,
        outboundDate: params.outboundDate || null,
        returnDate: params.returnDate || null,
        maxPrice: params.maxPrice,
        stops: params.stops !== 0 ? params.stops : null,
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
}


