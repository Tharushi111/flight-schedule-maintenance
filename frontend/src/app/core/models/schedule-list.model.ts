export interface ScheduleList {
  scheduleId: number;
  flightNumber: string;
  origin: string;
  destination: string;
  departureTime: string;
  arrivalTime: string;
  arrivesNextDay: boolean;
  aircraftType: string;
  daysOfOperation: string;
  effectiveFrom: string;
  effectiveTo: string | null;
  status: string;
}