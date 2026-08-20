import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Airport } from '../models/airport.model';

@Injectable({
  providedIn: 'root'
})
export class AirportService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    `${environment.apiUrl}/airports`;

  getAirports(): Observable<Airport[]> {
    return this.http.get<Airport[]>(
      this.apiUrl
    );
  }
}