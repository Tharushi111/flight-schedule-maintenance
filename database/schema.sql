CREATE TABLE Airport
(
    AirportId INT IDENTITY(1,1) NOT NULL,
    IataCode CHAR(3) NOT NULL,
    AirportName NVARCHAR(120) NOT NULL,
    City NVARCHAR(80) NOT NULL,
    CountryCode CHAR(2) NOT NULL,
    IsActive BIT NOT NULL
        CONSTRAINT DF_Airport_IsActive DEFAULT (1),

    CONSTRAINT PK_Airport
        PRIMARY KEY (AirportId),

    CONSTRAINT UQ_Airport_IataCode
        UNIQUE (IataCode)
);
GO

CREATE TABLE FlightSchedule
(
    ScheduleId INT IDENTITY(1,1) NOT NULL,

    FlightNumber VARCHAR(7) NOT NULL,

    OriginAirportId INT NOT NULL,
    DestinationAirportId INT NOT NULL,

    DepartureTime TIME(0) NOT NULL,
    ArrivalTime TIME(0) NOT NULL,

    AircraftType VARCHAR(10) NOT NULL,

    DaysOfOperation VARCHAR(7) NOT NULL,

    EffectiveFrom DATE NOT NULL,
    EffectiveTo DATE NULL,

    Status VARCHAR(12) NOT NULL,

    CreatedOn DATETIME2 NOT NULL,

    ModifiedOn DATETIME2 NULL,

    CONSTRAINT PK_FlightSchedule
        PRIMARY KEY (ScheduleId),

    CONSTRAINT UQ_FlightSchedule_FlightNumber_EffectiveFrom
        UNIQUE (FlightNumber, EffectiveFrom),

    CONSTRAINT CK_FlightSchedule_DifferentAirports
        CHECK (OriginAirportId <> DestinationAirportId),

    CONSTRAINT FK_FlightSchedule_OriginAirport
        FOREIGN KEY (OriginAirportId)
        REFERENCES Airport(AirportId),

    CONSTRAINT FK_FlightSchedule_DestinationAirport
        FOREIGN KEY (DestinationAirportId)
        REFERENCES Airport(AirportId)
);
GO

INSERT INTO Airport
(
    IataCode,
    AirportName,
    City,
    CountryCode,
    IsActive
)
VALUES
('CMB', 'Bandaranaike International Airport', 'Colombo', 'LK', 1),
('HRI', 'Mattala Rajapaksa International Airport', 'Hambantota', 'LK', 1),
('JAF', 'Jaffna International Airport', 'Jaffna', 'LK', 1),
('MLE', 'Velana International Airport', 'Male', 'MV', 1),
('SIN', 'Singapore Changi Airport', 'Singapore', 'SG', 1),
('KUL', 'Kuala Lumpur International Airport', 'Kuala Lumpur', 'MY', 1),
('BKK', 'Suvarnabhumi Airport', 'Bangkok', 'TH', 1),
('DEL', 'Indira Gandhi International Airport', 'Delhi', 'IN', 1),
('BOM', 'Chhatrapati Shivaji Maharaj International Airport', 'Mumbai', 'IN', 1),
('MAA', 'Chennai International Airport', 'Chennai', 'IN', 1),
('DXB', 'Dubai International Airport', 'Dubai', 'AE', 1),
('DOH', 'Hamad International Airport', 'Doha', 'QA', 1),
('LHR', 'Heathrow Airport', 'London', 'GB', 1),
('CDG', 'Charles de Gaulle Airport', 'Paris', 'FR', 1),
('FRA', 'Frankfurt Airport', 'Frankfurt', 'DE', 1);
GO