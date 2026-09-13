/* =====================================================================
   Car Rental Booking - Database Schema (SQL Server)
   Naming convention:
     mst_  -> master tables
     trn_  -> transaction tables
     tbl_  -> other / helper tables
   ===================================================================== */

IF DB_ID('CarRentalDB') IS NULL
BEGIN
    CREATE DATABASE CarRentalDB;
END
GO

USE CarRentalDB;
GO

/* ---------------------------------------------------------------------
   MASTER TABLES (mst_)
   --------------------------------------------------------------------- */

IF OBJECT_ID('dbo.mst_role', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mst_role (
        role_id        INT IDENTITY(1,1) PRIMARY KEY,
        role_name      NVARCHAR(50)   NOT NULL,
        created_date   DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID('dbo.mst_car_type', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mst_car_type (
        car_type_id    INT IDENTITY(1,1) PRIMARY KEY,
        type_name      NVARCHAR(50)   NOT NULL
    );
END
GO

IF OBJECT_ID('dbo.mst_location', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mst_location (
        location_id    INT IDENTITY(1,1) PRIMARY KEY,
        city_name      NVARCHAR(100)  NOT NULL
    );
END
GO

IF OBJECT_ID('dbo.mst_user', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mst_user (
        user_id        INT IDENTITY(1,1) PRIMARY KEY,
        full_name      NVARCHAR(100)  NOT NULL,
        email          NVARCHAR(150)  NOT NULL,
        password_hash  NVARCHAR(256)  NOT NULL,
        role_id        INT            NOT NULL,
        is_active      BIT            NOT NULL DEFAULT 1,
        created_date   DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_mst_user_email UNIQUE (email),
        CONSTRAINT FK_mst_user_role FOREIGN KEY (role_id) REFERENCES dbo.mst_role(role_id)
    );
END
GO

IF OBJECT_ID('dbo.mst_car', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mst_car (
        car_id         INT IDENTITY(1,1) PRIMARY KEY,
        car_name       NVARCHAR(100)  NOT NULL,
        brand          NVARCHAR(100)  NOT NULL,
        car_type_id    INT            NOT NULL,
        price_per_day  DECIMAL(10,2)  NOT NULL,
        seats          INT            NOT NULL,
        transmission   NVARCHAR(20)   NOT NULL,
        fuel_type      NVARCHAR(20)   NOT NULL,
        rating         DECIMAL(2,1)   NOT NULL DEFAULT 0,
        image_url      NVARCHAR(500)  NOT NULL,
        is_available   BIT            NOT NULL DEFAULT 1,
        location_id    INT            NOT NULL,
        created_date   DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_mst_car_type FOREIGN KEY (car_type_id) REFERENCES dbo.mst_car_type(car_type_id),
        CONSTRAINT FK_mst_car_location FOREIGN KEY (location_id) REFERENCES dbo.mst_location(location_id)
    );
END
GO

/* ---------------------------------------------------------------------
   TRANSACTION TABLES (trn_)
   --------------------------------------------------------------------- */

IF OBJECT_ID('dbo.trn_booking', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.trn_booking (
        booking_id     INT IDENTITY(1,1) PRIMARY KEY,
        booking_no     NVARCHAR(20)   NOT NULL,
        user_id        INT            NOT NULL,
        car_id         INT            NOT NULL,
        from_date      DATETIME2      NOT NULL,
        to_date        DATETIME2      NOT NULL,
        total_amount   DECIMAL(10,2)  NOT NULL,
        status         NVARCHAR(20)   NOT NULL DEFAULT 'Pending',
        created_date   DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_trn_booking_no UNIQUE (booking_no),
        CONSTRAINT FK_trn_booking_user FOREIGN KEY (user_id) REFERENCES dbo.mst_user(user_id),
        CONSTRAINT FK_trn_booking_car FOREIGN KEY (car_id) REFERENCES dbo.mst_car(car_id)
    );
END
GO

/* ---------------------------------------------------------------------
   OTHER / HELPER TABLES (tbl_)
   --------------------------------------------------------------------- */

IF OBJECT_ID('dbo.tbl_refresh_token', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_refresh_token (
        token_id       INT IDENTITY(1,1) PRIMARY KEY,
        user_id        INT            NOT NULL,
        token          NVARCHAR(500)  NOT NULL,
        expiry_date    DATETIME2      NOT NULL,
        is_revoked     BIT            NOT NULL DEFAULT 0,
        created_date   DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_tbl_refresh_token_user FOREIGN KEY (user_id) REFERENCES dbo.mst_user(user_id) ON DELETE CASCADE
    );
END
GO

/* =====================================================================
   SEED DATA
   ===================================================================== */

IF NOT EXISTS (SELECT 1 FROM dbo.mst_role)
BEGIN
    INSERT INTO dbo.mst_role (role_name) VALUES ('Admin'), ('Customer');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.mst_car_type)
BEGIN
    INSERT INTO dbo.mst_car_type (type_name)
    VALUES ('Sedan'), ('SUV'), ('Hatchback'), ('Luxury'), ('Electric');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.mst_location)
BEGIN
    INSERT INTO dbo.mst_location (city_name)
    VALUES ('Bhopal'), ('Indore'), ('Delhi'), ('Mumbai');
END
GO

-- Demo login -> email: demo@driveon.com | password: Demo@123
IF NOT EXISTS (SELECT 1 FROM dbo.mst_user WHERE email = 'demo@driveon.com')
BEGIN
    INSERT INTO dbo.mst_user (full_name, email, password_hash, role_id, is_active)
    VALUES (
        'Demo User',
        'demo@driveon.com',
        '$2b$11$aYdgWAKTu6uNfv1xmRe6Lubhkti5R4pil8Q7l6M86Li80PtL5j8hq',
        (SELECT role_id FROM dbo.mst_role WHERE role_name = 'Customer'),
        1
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.mst_car)
BEGIN
    INSERT INTO dbo.mst_car (car_name, brand, car_type_id, price_per_day, seats, transmission, fuel_type, rating, image_url, is_available, location_id)
    VALUES
    ('Swift Dzire',   'Maruti Suzuki', (SELECT car_type_id FROM dbo.mst_car_type WHERE type_name = 'Sedan'),     1800.00, 5, 'Manual',    'Petrol',   4.4, 'https://images.unsplash.com/photo-1541899481282-d53bffe3c35d?auto=format&fit=crop&w=600&q=80', 1, (SELECT location_id FROM dbo.mst_location WHERE city_name = 'Bhopal')),
    ('Creta',         'Hyundai',       (SELECT car_type_id FROM dbo.mst_car_type WHERE type_name = 'SUV'),       3200.00, 5, 'Automatic', 'Diesel',   4.7, 'https://images.unsplash.com/photo-1553440569-bcc63803a83d?auto=format&fit=crop&w=600&q=80', 1, (SELECT location_id FROM dbo.mst_location WHERE city_name = 'Indore')),
    ('Model 3',       'Tesla',         (SELECT car_type_id FROM dbo.mst_car_type WHERE type_name = 'Electric'),  6500.00, 5, 'Automatic', 'Electric', 4.9, 'https://images.unsplash.com/photo-1560958089-b8a1929cea89?auto=format&fit=crop&w=600&q=80', 0, (SELECT location_id FROM dbo.mst_location WHERE city_name = 'Delhi')),
    ('City',          'Honda',         (SELECT car_type_id FROM dbo.mst_car_type WHERE type_name = 'Sedan'),     2600.00, 5, 'Automatic', 'Petrol',   4.5, 'https://images.unsplash.com/photo-1622194993926-2f30edb5f9a7?auto=format&fit=crop&w=600&q=80', 1, (SELECT location_id FROM dbo.mst_location WHERE city_name = 'Bhopal')),
    ('Fortuner',      'Toyota',        (SELECT car_type_id FROM dbo.mst_car_type WHERE type_name = 'SUV'),       5800.00, 7, 'Automatic', 'Diesel',   4.8, 'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?auto=format&fit=crop&w=600&q=80', 1, (SELECT location_id FROM dbo.mst_location WHERE city_name = 'Mumbai')),
    ('Baleno',        'Maruti Suzuki', (SELECT car_type_id FROM dbo.mst_car_type WHERE type_name = 'Hatchback'), 1500.00, 5, 'Manual',    'Petrol',   4.2, 'https://images.unsplash.com/photo-1605559424843-9e4c228bf1c2?auto=format&fit=crop&w=600&q=80', 1, (SELECT location_id FROM dbo.mst_location WHERE city_name = 'Indore')),
    ('BMW 5 Series',  'BMW',           (SELECT car_type_id FROM dbo.mst_car_type WHERE type_name = 'Luxury'),    9000.00, 5, 'Automatic', 'Petrol',   4.9, 'https://images.unsplash.com/photo-1555215695-3004980ad54e?auto=format&fit=crop&w=600&q=80', 1, (SELECT location_id FROM dbo.mst_location WHERE city_name = 'Delhi')),
    ('Nexon EV',      'Tata',          (SELECT car_type_id FROM dbo.mst_car_type WHERE type_name = 'Electric'),  2900.00, 5, 'Automatic', 'Electric', 4.3, 'https://images.unsplash.com/photo-1617469767053-d3b523a0b982?auto=format&fit=crop&w=600&q=80', 1, (SELECT location_id FROM dbo.mst_location WHERE city_name = 'Bhopal'));
END
GO

PRINT 'CarRentalDB schema + seed data created successfully.';
