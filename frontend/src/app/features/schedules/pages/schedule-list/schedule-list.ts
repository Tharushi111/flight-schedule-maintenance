import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  signal
} from '@angular/core';

import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { Airport } from '../../../../core/models/airport.model';
import { ScheduleList as ScheduleListModel } from '../../../../core/models/schedule-list.model';

import { AirportService } from '../../../../core/services/airport.service';
import { ScheduleService } from '../../../../core/services/schedule.service';

@Component({
  selector: 'app-schedule-list',
  standalone: true,
  imports: [
    ReactiveFormsModule
  ],
  templateUrl: './schedule-list.html',
  styleUrl: './schedule-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ScheduleList {
  private readonly formBuilder = inject(FormBuilder);
  private readonly airportService = inject(AirportService);
  private readonly scheduleService = inject(ScheduleService);
  private readonly destroyRef = inject(DestroyRef);

  readonly airports = signal<Airport[]>([]);
  readonly schedules = signal<ScheduleListModel[]>([]);

  readonly loading = signal(false);
  readonly airportsLoading = signal(false);

  readonly errorMessage = signal('');
  readonly statusUpdatingId = signal<number | null>(null);

  readonly statuses = [
    'Draft',
    'Published',
    'Suspended'
  ];

  readonly filterForm = this.formBuilder.group({
    originAirportId:
      this.formBuilder.control<number | null>(null),

    destinationAirportId:
      this.formBuilder.control<number | null>(null),

    status:
      this.formBuilder.control<string>('')
  });

  constructor() {
    this.loadAirports();
    this.loadSchedules();
  }


  // =========================================================
  // LOAD AIRPORTS
  // =========================================================

  private loadAirports(): void {
    this.airportsLoading.set(true);

    this.airportService
      .getAirports()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.airportsLoading.set(false);
        })
      )
      .subscribe({
        next: (airports) => {
          this.airports.set(airports);
        },

        error: () => {
          this.errorMessage.set(
            'Unable to load airport information.'
          );
        }
      });
  }


  // =========================================================
  // LOAD SCHEDULES
  // =========================================================

  loadSchedules(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    const {
      originAirportId,
      destinationAirportId,
      status
    } = this.filterForm.getRawValue();

    this.scheduleService
      .getSchedules(
        originAirportId,
        destinationAirportId,
        status
      )
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.loading.set(false);
        })
      )
      .subscribe({
        next: (schedules) => {
          this.schedules.set(schedules);
        },

        error: (error) => {
          console.error(
            'Failed to load schedules:',
            error
          );

          this.errorMessage.set(
            'Unable to load flight schedules.'
          );
        }
      });
  }


  // =========================================================
  // APPLY FILTERS
  // =========================================================

  applyFilters(): void {
    this.loadSchedules();
  }


  // =========================================================
  // CLEAR FILTERS
  // =========================================================

  clearFilters(): void {
    this.filterForm.reset({
      originAirportId: null,
      destinationAirportId: null,
      status: ''
    });

    this.loadSchedules();
  }


  // =========================================================
  // INLINE STATUS UPDATE
  // =========================================================

  changeStatus(
    schedule: ScheduleListModel,
    status: string
  ): void {
    if (!status || status === schedule.status) {
      return;
    }

    this.statusUpdatingId.set(
      schedule.scheduleId
    );

    this.errorMessage.set('');

    this.scheduleService
      .updateScheduleStatus(
        schedule.scheduleId,
        { status }
      )
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.statusUpdatingId.set(null);
        })
      )
      .subscribe({
        next: (updatedSchedule) => {
          this.schedules.update(
            schedules =>
              schedules.map(item =>
                item.scheduleId ===
                updatedSchedule.scheduleId
                  ? {
                      ...item,
                      status:
                        updatedSchedule.status
                    }
                  : item
              )
          );
        },

        error: (error) => {
          console.error(
            'Failed to update schedule status:',
            error
          );

          this.errorMessage.set(
            'Unable to update schedule status.'
          );
        }
      });
  }
}