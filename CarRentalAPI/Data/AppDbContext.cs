using CarRentalAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRentalAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Master tables (mst_)
    public DbSet<MstRole> MstRoles => Set<MstRole>();
    public DbSet<MstUser> MstUsers => Set<MstUser>();
    public DbSet<MstCarType> MstCarTypes => Set<MstCarType>();
    public DbSet<MstLocation> MstLocations => Set<MstLocation>();
    public DbSet<MstCar> MstCars => Set<MstCar>();
    public DbSet<MstDriver> MstDrivers => Set<MstDriver>();
    public DbSet<MstLocationDistance> MstLocationDistances => Set<MstLocationDistance>();

    // Transaction tables (trn_)
    public DbSet<TrnBooking> TrnBookings => Set<TrnBooking>();
    public DbSet<TrnBookingDetail> TrnBookingDetails => Set<TrnBookingDetail>();

    // Other / helper tables (tbl_)
    public DbSet<TblRefreshToken> TblRefreshTokens => Set<TblRefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique constraints
        modelBuilder.Entity<MstUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<TrnBooking>()
            .HasIndex(b => b.BookingNo)
            .IsUnique();

        // Relationships / delete behavior
        modelBuilder.Entity<TrnBooking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrnBooking>()
            .HasOne(b => b.Car)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.CarId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MstCar>()
            .HasOne(c => c.CarType)
            .WithMany(t => t.Cars)
            .HasForeignKey(c => c.CarTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MstCar>()
            .HasOne(c => c.Location)
            .WithMany(l => l.Cars)
            .HasForeignKey(c => c.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MstCar>()
            .HasOne(c => c.AssignedDriver)
            .WithMany(d => d.Cars)
            .HasForeignKey(c => c.AssignedDriverId)
            .OnDelete(DeleteBehavior.Restrict);

        // A location-distance row is unique per (from, to) direction; both
        // directions are seeded explicitly so lookups never need to guess order.
        modelBuilder.Entity<MstLocationDistance>()
            .HasOne(ld => ld.FromLocation)
            .WithMany()
            .HasForeignKey(ld => ld.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MstLocationDistance>()
            .HasOne(ld => ld.ToLocation)
            .WithMany()
            .HasForeignKey(ld => ld.ToLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MstLocationDistance>()
            .HasIndex(ld => new { ld.FromLocationId, ld.ToLocationId })
            .IsUnique();

        // trn_booking_detail: one-to-one with trn_booking (booking_id is both
        // the FK and, via this configuration, a unique key on the child row).
        modelBuilder.Entity<TrnBooking>()
            .HasOne(b => b.Detail)
            .WithOne(d => d.Booking)
            .HasForeignKey<TrnBookingDetail>(d => d.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TrnBookingDetail>()
            .HasOne(d => d.FromLocation)
            .WithMany()
            .HasForeignKey(d => d.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrnBookingDetail>()
            .HasOne(d => d.ToLocation)
            .WithMany()
            .HasForeignKey(d => d.ToLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrnBookingDetail>()
            .HasOne(d => d.Driver)
            .WithMany()
            .HasForeignKey(d => d.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MstUser>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TblRefreshToken>()
            .HasOne(t => t.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ---------- Seed data ----------

        modelBuilder.Entity<MstRole>().HasData(
            new MstRole { RoleId = 1, RoleName = "Admin", CreatedDate = new DateTime(2026, 1, 1) },
            new MstRole { RoleId = 2, RoleName = "Customer", CreatedDate = new DateTime(2026, 1, 1) }
        );

        modelBuilder.Entity<MstCarType>().HasData(
            new MstCarType { CarTypeId = 1, TypeName = "Sedan" },
            new MstCarType { CarTypeId = 2, TypeName = "SUV" },
            new MstCarType { CarTypeId = 3, TypeName = "Hatchback" },
            new MstCarType { CarTypeId = 4, TypeName = "Luxury" },
            new MstCarType { CarTypeId = 5, TypeName = "Electric" }
        );

        modelBuilder.Entity<MstLocation>().HasData(
            new MstLocation { LocationId = 1, CityName = "Bhopal" },
            new MstLocation { LocationId = 2, CityName = "Indore" },
            new MstLocation { LocationId = 3, CityName = "Delhi" },
            new MstLocation { LocationId = 4, CityName = "Mumbai" }
        );

        // Demo user -> email: demo@driveon.com / password: Demo@123
        // Hash generated with BCrypt.Net-Next (work factor 11)
        // Admin user -> email: admin@driveon.com / password: Admin@123
        // Hash generated with the same bcrypt work factor (11), same login flow -
        // just seeded with RoleId = 1 (Admin) instead of 2 (Customer).
        modelBuilder.Entity<MstUser>().HasData(
            new MstUser
            {
                UserId = 1,
                FullName = "Demo User",
                Email = "demo@driveon.com",
                PasswordHash = "$2b$11$aYdgWAKTu6uNfv1xmRe6Lubhkti5R4pil8Q7l6M86Li80PtL5j8hq",
                RoleId = 2,
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1)
            },
            new MstUser
            {
                UserId = 2,
                FullName = "Admin",
                Email = "admin@driveon.com",
                PasswordHash = "$2b$11$qmTwlHntqavQcV.VACsh6uD6boPH4y7Lv5SKTobfzOICgof6c.OSW",
                RoleId = 1,
                IsActive = true,
                CreatedDate = new DateTime(2026, 1, 1)
            }
        );

        modelBuilder.Entity<MstCar>().HasData(
            new MstCar { CarId = 1, CarName = "Swift Dzire", Brand = "Maruti Suzuki", CarTypeId = 1, PricePerDay = 1800m, Seats = 5, Transmission = "Manual", FuelType = "Petrol", Rating = 4.4m, ImageUrl = "https://images.unsplash.com/photo-1541899481282-d53bffe3c35d?auto=format&fit=crop&w=600&q=80", IsAvailable = true, LocationId = 1, AssignedDriverId = 1, CreatedDate = new DateTime(2026, 1, 1) },
            new MstCar { CarId = 2, CarName = "Creta", Brand = "Hyundai", CarTypeId = 2, PricePerDay = 3200m, Seats = 5, Transmission = "Automatic", FuelType = "Diesel", Rating = 4.7m, ImageUrl = "https://images.unsplash.com/photo-1553440569-bcc63803a83d?auto=format&fit=crop&w=600&q=80", IsAvailable = true, LocationId = 2, AssignedDriverId = 2, CreatedDate = new DateTime(2026, 1, 1) },
            new MstCar { CarId = 3, CarName = "Model 3", Brand = "Tesla", CarTypeId = 5, PricePerDay = 6500m, Seats = 5, Transmission = "Automatic", FuelType = "Electric", Rating = 4.9m, ImageUrl = "https://images.unsplash.com/photo-1560958089-b8a1929cea89?auto=format&fit=crop&w=600&q=80", IsAvailable = false, LocationId = 3, AssignedDriverId = 3, CreatedDate = new DateTime(2026, 1, 1) },
            new MstCar { CarId = 4, CarName = "City", Brand = "Honda", CarTypeId = 1, PricePerDay = 2600m, Seats = 5, Transmission = "Automatic", FuelType = "Petrol", Rating = 4.5m, ImageUrl = "https://images.unsplash.com/photo-1622194993926-2f30edb5f9a7?auto=format&fit=crop&w=600&q=80", IsAvailable = true, LocationId = 1, AssignedDriverId = 4, CreatedDate = new DateTime(2026, 1, 1) },
            new MstCar { CarId = 5, CarName = "Fortuner", Brand = "Toyota", CarTypeId = 2, PricePerDay = 5800m, Seats = 7, Transmission = "Automatic", FuelType = "Diesel", Rating = 4.8m, ImageUrl = "https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?auto=format&fit=crop&w=600&q=80", IsAvailable = true, LocationId = 4, AssignedDriverId = 5, CreatedDate = new DateTime(2026, 1, 1) },
            new MstCar { CarId = 6, CarName = "Baleno", Brand = "Maruti Suzuki", CarTypeId = 3, PricePerDay = 1500m, Seats = 5, Transmission = "Manual", FuelType = "Petrol", Rating = 4.2m, ImageUrl = "https://images.unsplash.com/photo-1605559424843-9e4c228bf1c2?auto=format&fit=crop&w=600&q=80", IsAvailable = true, LocationId = 2, AssignedDriverId = 6, CreatedDate = new DateTime(2026, 1, 1) },
            new MstCar { CarId = 7, CarName = "BMW 5 Series", Brand = "BMW", CarTypeId = 4, PricePerDay = 9000m, Seats = 5, Transmission = "Automatic", FuelType = "Petrol", Rating = 4.9m, ImageUrl = "https://images.unsplash.com/photo-1555215695-3004980ad54e?auto=format&fit=crop&w=600&q=80", IsAvailable = true, LocationId = 3, AssignedDriverId = 7, CreatedDate = new DateTime(2026, 1, 1) },
            new MstCar { CarId = 8, CarName = "Nexon EV", Brand = "Tata", CarTypeId = 5, PricePerDay = 2900m, Seats = 5, Transmission = "Automatic", FuelType = "Electric", Rating = 4.3m, ImageUrl = "https://images.unsplash.com/photo-1617469767053-d3b523a0b982?auto=format&fit=crop&w=600&q=80", IsAvailable = true, LocationId = 1, AssignedDriverId = 8, CreatedDate = new DateTime(2026, 1, 1) }
        );

        // Fixed drivers - one already assigned per car (see AssignedDriverId above).
        modelBuilder.Entity<MstDriver>().HasData(
            new MstDriver { DriverId = 1, DriverName = "Ramesh Yadav", PhoneNumber = "9800000001", LicenseNumber = "MP-DL-000001", IsActive = true, CreatedDate = new DateTime(2026, 1, 1) },
            new MstDriver { DriverId = 2, DriverName = "Suresh Kumar", PhoneNumber = "9800000002", LicenseNumber = "MP-DL-000002", IsActive = true, CreatedDate = new DateTime(2026, 1, 1) },
            new MstDriver { DriverId = 3, DriverName = "Anil Verma", PhoneNumber = "9800000003", LicenseNumber = "DL-DL-000003", IsActive = true, CreatedDate = new DateTime(2026, 1, 1) },
            new MstDriver { DriverId = 4, DriverName = "Vikram Singh", PhoneNumber = "9800000004", LicenseNumber = "MP-DL-000004", IsActive = true, CreatedDate = new DateTime(2026, 1, 1) },
            new MstDriver { DriverId = 5, DriverName = "Rajesh Chauhan", PhoneNumber = "9800000005", LicenseNumber = "MH-DL-000005", IsActive = true, CreatedDate = new DateTime(2026, 1, 1) },
            new MstDriver { DriverId = 6, DriverName = "Manoj Tiwari", PhoneNumber = "9800000006", LicenseNumber = "MP-DL-000006", IsActive = true, CreatedDate = new DateTime(2026, 1, 1) },
            new MstDriver { DriverId = 7, DriverName = "Sanjay Mehta", PhoneNumber = "9800000007", LicenseNumber = "DL-DL-000007", IsActive = true, CreatedDate = new DateTime(2026, 1, 1) },
            new MstDriver { DriverId = 8, DriverName = "Deepak Rathore", PhoneNumber = "9800000008", LicenseNumber = "MP-DL-000008", IsActive = true, CreatedDate = new DateTime(2026, 1, 1) }
        );

        // Approx. road distance (km) between every location pair, both directions.
        // Locations: 1=Bhopal, 2=Indore, 3=Delhi, 4=Mumbai. Same from/to location
        // is treated as 0km in code (no row needed) - see BookingsController.
        modelBuilder.Entity<MstLocationDistance>().HasData(
            new MstLocationDistance { DistanceId = 1, FromLocationId = 1, ToLocationId = 2, DistanceKm = 190m },
            new MstLocationDistance { DistanceId = 2, FromLocationId = 2, ToLocationId = 1, DistanceKm = 190m },
            new MstLocationDistance { DistanceId = 3, FromLocationId = 1, ToLocationId = 3, DistanceKm = 740m },
            new MstLocationDistance { DistanceId = 4, FromLocationId = 3, ToLocationId = 1, DistanceKm = 740m },
            new MstLocationDistance { DistanceId = 5, FromLocationId = 1, ToLocationId = 4, DistanceKm = 780m },
            new MstLocationDistance { DistanceId = 6, FromLocationId = 4, ToLocationId = 1, DistanceKm = 780m },
            new MstLocationDistance { DistanceId = 7, FromLocationId = 2, ToLocationId = 3, DistanceKm = 820m },
            new MstLocationDistance { DistanceId = 8, FromLocationId = 3, ToLocationId = 2, DistanceKm = 820m },
            new MstLocationDistance { DistanceId = 9, FromLocationId = 2, ToLocationId = 4, DistanceKm = 580m },
            new MstLocationDistance { DistanceId = 10, FromLocationId = 4, ToLocationId = 2, DistanceKm = 580m },
            new MstLocationDistance { DistanceId = 11, FromLocationId = 3, ToLocationId = 4, DistanceKm = 1400m },
            new MstLocationDistance { DistanceId = 12, FromLocationId = 4, ToLocationId = 3, DistanceKm = 1400m }
        );
    }
}
