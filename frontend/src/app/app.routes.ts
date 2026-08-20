import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'schedules'
  },

  {
    path: 'schedules',
    loadComponent: () =>
      import(
        './features/schedules/pages/schedule-list/schedule-list'
      ).then(
        component =>
          component.ScheduleList
      )
  },

  {
    path: 'schedules/new',
    loadComponent: () =>
      import(
        './features/schedules/pages/schedule-form/schedule-form'
      ).then(
        component =>
          component.ScheduleForm
      )
  },

  {
    path: 'schedules/:id/edit',
    loadComponent: () =>
      import(
        './features/schedules/pages/schedule-form/schedule-form'
      ).then(
        component =>
          component.ScheduleForm
      )
  },

  {
    path: '**',
    redirectTo: 'schedules'
  }
];