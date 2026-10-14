<<<<<<< HEAD
import { LowCostFlightFlightsCreateFlightDealCreateFlightDealRequest } from '../../../core/api/models';
=======
import { LowCostFlightFlightsCreateFlightDealCreateFlightDealRequest } from "../../../core/api/models";
>>>>>>> e01c44ab014ac8f22451b6eb91066a29fafa748d

export interface FlightSearchParams {
  departureId: string;
  arrivalId: string | null;
  currency: string;
  outboundDate: string | null;
  returnDate: string | null;
  maxPrice: number | null;
  stops: number | null;
}

export const DEFAULT_SEARCH_PARAMS: FlightSearchParams = {
  departureId: 'MXP',
  arrivalId: '',
  currency: 'EUR',
  outboundDate: '',
  returnDate: '',
  maxPrice: null,
  stops: 0,
};

<<<<<<< HEAD
=======

>>>>>>> e01c44ab014ac8f22451b6eb91066a29fafa748d
export const DEFAULT_DEAL: LowCostFlightFlightsCreateFlightDealCreateFlightDealRequest = {
  departureAirport: '',
  arrivalAirport: '',
  nameCity: '',
  country: '',
  price: 0,
  averagePrice: 0,
  currency: 'EUR',
  airline: '',
  durationInMinutes: 0,
  stops: 0,
  flightLink: '',
  description: '',
<<<<<<< HEAD
};
=======
};
>>>>>>> e01c44ab014ac8f22451b6eb91066a29fafa748d
