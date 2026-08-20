import { inject, Injectable } from '@angular/core';
import {
  HttpClient,
  HttpParams
} from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

import { ScheduleList } from '../models/schedule-list.model';
import { ScheduleDetails } from '../models/schedule-details.model';
import { CreateScheduleRequest } from '../models/create-schedule-request.model';
import { UpdateScheduleRequest } from '../models/update-schedule-request.model';
import { UpdateScheduleStatusRequest } from '../models/update-schedule-status-request.model';

@Injectable({
  providedIn: 'root'
})
export class ScheduleService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    `${environment.apiUrl}/schedules`;


  getSchedules(
    originAirportId?: number | null,
    destinationAirportId?: number | null,
    status?: string | null
  ): Observable<ScheduleList[]> {
    let params = new HttpParams();

    if (originAirportId != null) {
      params = params.set(
        'origin',
        originAirportId.toString()
      );
    }

    if (destinationAirportId != null) {
      params = params.set(
        'destination',
        destinationAirportId.toString()
      );
    }

    if (status?.trim()) {
      params = params.set(
        'status',
        status.trim()
      );
    }

    return this.http.get<ScheduleList[]>(
      this.apiUrl,
      { params }
    );
  }


  getScheduleById(
    scheduleId: number
  ): Observable<ScheduleDetails> {
    return this.http.get<ScheduleDetails>(
      `${this.apiUrl}/${scheduleId}`
    );
  }


  createSchedule(
    request: CreateScheduleRequest
  ): Observable<ScheduleDetails> {
    return this.http.post<ScheduleDetails>(
      this.apiUrl,
      request
    );
  }


  updateSchedule(
    scheduleId: number,
    request: UpdateScheduleRequest
  ): Observable<ScheduleDetails> {
    return this.http.put<ScheduleDetails>(
      `${this.apiUrl}/${scheduleId}`,
      request
    );
  }


  updateScheduleStatus(
    scheduleId: number,
    request: UpdateScheduleStatusRequest
  ): Observable<ScheduleDetails> {
    return this.http.patch<ScheduleDetails>(
      `${this.apiUrl}/${scheduleId}/status`,
      request
    );
  }


  deleteSchedule(
    scheduleId: number
  ): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${scheduleId}`
    );
  }
}