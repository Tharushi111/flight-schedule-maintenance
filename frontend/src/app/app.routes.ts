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
    path: '**',
    redirectTo: 'schedules'
  }
];