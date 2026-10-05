import { Component } from '@angular/core';
import { FlightSearchComponent } from './feature/flights/components/flight-search/flight-search';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [FlightSearchComponent],
  templateUrl: './app.html'
})
export class App { }
