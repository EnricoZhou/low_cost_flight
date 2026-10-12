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
