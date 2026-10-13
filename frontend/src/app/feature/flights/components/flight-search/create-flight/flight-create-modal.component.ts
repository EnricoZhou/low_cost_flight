import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { LowCostFlightFlightsCreateFlightDealCreateFlightDealRequest } from '../../../../../core/api/models';
import { DEFAULT_DEAL } from '../../../models';

@Component({
  standalone: true,
  selector: 'app-flight-create-modal',
  templateUrl: './flight-create-modal.component.html',
  imports: [CommonModule, FormsModule],
})
export class FlightCreateModalComponent {
  readonly isSubmittingDeal = input<boolean>(false);
  readonly closeCreateDealModal = output<void>();
  readonly createFlightDeal = output<LowCostFlightFlightsCreateFlightDealCreateFlightDealRequest>();
  newDeal: LowCostFlightFlightsCreateFlightDealCreateFlightDealRequest = { ...DEFAULT_DEAL };

  onCancel(): void {
    this.closeCreateDealModal.emit();
  }

  onSubmit(): void {
    if (!this.newDeal.departureAirport || !this.newDeal.arrivalAirport || !this.newDeal.price) {
      return;
    }
    this.createFlightDeal.emit(this.newDeal);
  }
}
