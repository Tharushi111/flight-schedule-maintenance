export interface CreateScheduleRequest {
  flightNumber: string;
  originAirportId: number;
  destinationAirportId: number;
  departureTime: string;
  arrivalTime: string;
  aircraftType: string;
  daysOfOperation: string;
  effectiveFrom: string;
  effectiveTo: string | null;
  status: string;
}