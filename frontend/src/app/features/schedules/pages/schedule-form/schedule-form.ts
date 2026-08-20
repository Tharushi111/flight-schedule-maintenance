import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal
} from '@angular/core';

import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import {
  ActivatedRoute,
  Router,
  RouterLink
} from '@angular/router';

import {
  HttpErrorResponse
} from '@angular/common/http';

import {
  finalize
} from 'rxjs';

import {
  takeUntilDestroyed
} from '@angular/core/rxjs-interop';

import {
  Airport
} from '../../../../core/models/airport.model';

import {
  CreateScheduleRequest
} from '../../../../core/models/create-schedule-request.model';

import {
  UpdateScheduleRequest
} from '../../../../core/models/update-schedule-request.model';

import {
  AirportService
} from '../../../../core/services/airport.service';

import {
  ScheduleService
} from '../../../../core/services/schedule.service';

import {
  differentAirportsValidator,
  effectiveDateValidator
} from '../../validators/schedule.validators';


@Component({
  selector: 'app-schedule-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './schedule-form.html',
  styleUrl: './schedule-form.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ScheduleForm {
  private readonly formBuilder =
    inject(FormBuilder);

  private readonly airportService =
    inject(AirportService);

  private readonly scheduleService =
    inject(ScheduleService);

  private readonly route =
    inject(ActivatedRoute);

  private readonly router =
    inject(Router);

  private readonly destroyRef =
    inject(DestroyRef);


  readonly airports =
    signal<Airport[]>([]);

  readonly loading =
    signal(false);

  readonly saving =
    signal(false);

  readonly errorMessage =
    signal('');

  readonly scheduleId =
    signal<number | null>(null);


  readonly isEditMode =
    computed(
      () => this.scheduleId() !== null
    );

  readonly pageTitle =
    computed(
      () =>
        this.isEditMode()
          ? 'Edit Flight Schedule'
          : 'Create Flight Schedule'
    );

  readonly saveButtonText =
    computed(() => {
      if (this.saving()) {
        return 'Saving...';
      }

      return this.isEditMode()
        ? 'Save Changes'
        : 'Create Schedule';
    });


  readonly aircraftTypes = [
    'A320',
    'A330',
    'A350'
  ];

  readonly statuses = [
    'Draft',
    'Published',
    'Suspended'
  ];

  readonly days = [
    {
      key: 'monday',
      label: 'Monday',
      position: 1
    },
    {
      key: 'tuesday',
      label: 'Tuesday',
      position: 2
    },
    {
      key: 'wednesday',
      label: 'Wednesday',
      position: 3
    },
    {
      key: 'thursday',
      label: 'Thursday',
      position: 4
    },
    {
      key: 'friday',
      label: 'Friday',
      position: 5
    },
    {
      key: 'saturday',
      label: 'Saturday',
      position: 6
    },
    {
      key: 'sunday',
      label: 'Sunday',
      position: 7
    }
  ] as const;


  readonly form =
    this.formBuilder.group(
      {
        flightNumber:
          this.formBuilder.nonNullable.control(
            '',
            [
              Validators.required,
              Validators.pattern(
                /^[A-Za-z]{2}\d{3,4}$/
              )
            ]
          ),

        originAirportId:
          this.formBuilder.control<number | null>(
            null,
            Validators.required
          ),

        destinationAirportId:
          this.formBuilder.control<number | null>(
            null,
            Validators.required
          ),

        departureTime:
          this.formBuilder.nonNullable.control(
            '',
            Validators.required
          ),

        arrivalTime:
          this.formBuilder.nonNullable.control(
            '',
            Validators.required
          ),

        aircraftType:
          this.formBuilder.nonNullable.control(
            '',
            Validators.required
          ),

        effectiveFrom:
          this.formBuilder.nonNullable.control(
            '',
            Validators.required
          ),

        effectiveTo:
          this.formBuilder.nonNullable.control(
            ''
          ),

        status:
          this.formBuilder.nonNullable.control(
            'Draft',
            Validators.required
          ),

        days:
          this.formBuilder.group({
            monday:
              this.formBuilder.nonNullable.control(
                false
              ),

            tuesday:
              this.formBuilder.nonNullable.control(
                false
              ),

            wednesday:
              this.formBuilder.nonNullable.control(
                false
              ),

            thursday:
              this.formBuilder.nonNullable.control(
                false
              ),

            friday:
              this.formBuilder.nonNullable.control(
                false
              ),

            saturday:
              this.formBuilder.nonNullable.control(
                false
              ),

            sunday:
              this.formBuilder.nonNullable.control(
                false
              )
          })
      },
      {
        validators: [
          differentAirportsValidator,
          effectiveDateValidator
        ]
      }
    );


  constructor() {
    this.loadAirports();
    this.detectMode();
  }


  private detectMode(): void {
    const idParameter =
      this.route.snapshot.paramMap.get('id');

    if (!idParameter) {
      return;
    }

    const id =
      Number(idParameter);

    if (!Number.isInteger(id) || id <= 0) {
      this.errorMessage.set(
        'Invalid schedule ID.'
      );

      return;
    }

    this.scheduleId.set(id);

    this.loadSchedule(id);
  }


  private loadAirports(): void {
    this.airportService
      .getAirports()
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: airports => {
          this.airports.set(airports);
        },

        error: error => {
          console.error(
            'Failed to load airports:',
            error
          );

          this.errorMessage.set(
            'Unable to load airport information.'
          );
        }
      });
  }


  private loadSchedule(
    scheduleId: number
  ): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.scheduleService
      .getScheduleById(scheduleId)
      .pipe(
        takeUntilDestroyed(this.destroyRef),

        finalize(() => {
          this.loading.set(false);
        })
      )
      .subscribe({
        next: schedule => {
          this.form.patchValue({
            flightNumber:
              schedule.flightNumber,

            originAirportId:
              schedule.originAirportId,

            destinationAirportId:
              schedule.destinationAirportId,

            departureTime:
              this.toTimeInput(
                schedule.departureTime
              ),

            arrivalTime:
              this.toTimeInput(
                schedule.arrivalTime
              ),

            aircraftType:
              schedule.aircraftType,

            effectiveFrom:
              this.toDateInput(
                schedule.effectiveFrom
              ),

            effectiveTo:
              schedule.effectiveTo
                ? this.toDateInput(
                    schedule.effectiveTo
                  )
                : '',

            status:
              schedule.status
          });

          this.applyDaysFromStoredValue(
            schedule.daysOfOperation
          );
        },

        error: error => {
          console.error(
            'Failed to load schedule:',
            error
          );

          this.errorMessage.set(
            'Unable to load the selected schedule.'
          );
        }
      });
  }


  submit(): void {
    this.errorMessage.set('');

    if (this.form.invalid) {
      this.form.markAllAsTouched();

      return;
    }

    const daysOfOperation =
      this.buildDaysOfOperation();

    if (!daysOfOperation) {
      this.errorMessage.set(
        'Select at least one operating day.'
      );

      return;
    }

    const values =
      this.form.getRawValue();

    const request:
      CreateScheduleRequest = {
        flightNumber:
          values.flightNumber
            .trim()
            .toUpperCase(),

        originAirportId:
          values.originAirportId!,

        destinationAirportId:
          values.destinationAirportId!,

        departureTime:
          this.toApiTime(
            values.departureTime
          ),

        arrivalTime:
          this.toApiTime(
            values.arrivalTime
          ),

        aircraftType:
          values.aircraftType,

        daysOfOperation,

        effectiveFrom:
          values.effectiveFrom,

        effectiveTo:
          values.effectiveTo || null,

        status:
          values.status
      };

    this.saving.set(true);

    const id =
      this.scheduleId();

    const request$ =
      id === null
        ? this.scheduleService
            .createSchedule(request)
        : this.scheduleService
            .updateSchedule(
              id,
              request as UpdateScheduleRequest
            );

    request$
      .pipe(
        takeUntilDestroyed(this.destroyRef),

        finalize(() => {
          this.saving.set(false);
        })
      )
      .subscribe({
        next: () => {
          this.router.navigate([
            '/schedules'
          ]);
        },

        error: error => {
          this.handleApiError(error);
        }
      });
  }


  private buildDaysOfOperation(): string {
    const values =
      this.form.controls.days.getRawValue();

    const selected = [
      values.monday,
      values.tuesday,
      values.wednesday,
      values.thursday,
      values.friday,
      values.saturday,
      values.sunday
    ];

    if (!selected.some(Boolean)) {
      return '';
    }

    return selected
      .map(
        (checked, index) =>
          checked
            ? String(index + 1)
            : '.'
      )
      .join('');
  }


  private applyDaysFromStoredValue(
    value: string
  ): void {
    if (!value || value.length !== 7) {
      return;
    }

    this.form.controls.days.patchValue({
      monday:
        value[0] === '1',

      tuesday:
        value[1] === '2',

      wednesday:
        value[2] === '3',

      thursday:
        value[3] === '4',

      friday:
        value[4] === '5',

      saturday:
        value[5] === '6',

      sunday:
        value[6] === '7'
    });
  }


  private toApiTime(
    value: string
  ): string {
    if (value.length === 5) {
      return `${value}:00`;
    }

    return value;
  }


  private toTimeInput(
    value: string
  ): string {
    return value.substring(0, 5);
  }


  private toDateInput(
    value: string
  ): string {
    return value.substring(0, 10);
  }


  private handleApiError(
    error: HttpErrorResponse
  ): void {
    console.error(
      'Schedule save failed:',
      error
    );

    if (
      typeof error.error?.message ===
      'string'
    ) {
      this.errorMessage.set(
        error.error.message
      );

      return;
    }

    const validationErrors =
      error.error?.errors;

    if (validationErrors) {
      const firstKey =
        Object.keys(
          validationErrors
        )[0];

      const firstMessage =
        validationErrors[firstKey]?.[0];

      if (firstMessage) {
        this.errorMessage.set(
          firstMessage
        );

        return;
      }
    }

    this.errorMessage.set(
      'Unable to save the flight schedule.'
    );
  }
}