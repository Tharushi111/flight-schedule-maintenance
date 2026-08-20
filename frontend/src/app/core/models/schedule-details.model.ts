export interface ScheduleDetails {
  scheduleId: number;
  flightNumber: string;
  originAirportId: number;
  destinationAirportId: number;
  departureTime: string;
  arrivalTime: string;
  arrivesNextDay: boolean;
  aircraftType: string;
  daysOfOperation: string;
  effectiveFrom: string;
  effectiveTo: string | null;
  status: string;
  createdOn: string;
  modifiedOn: string | null;
}