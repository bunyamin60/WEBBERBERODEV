using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WEBBERBERODEV.Models;

namespace WEBBERBERODEV.DATA
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Salon> Salonlar { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Calisan> Calisanlar { get; set; }
        public DbSet<CalisanHizmet> CalisanHizmetler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CalisanHizmet tablo ayarları
            modelBuilder.Entity<CalisanHizmet>()
                .HasKey(ch => new { ch.CalisanId, ch.HizmetId });

            modelBuilder.Entity<CalisanHizmet>()
                .HasOne(ch => ch.Calisan)
                .WithMany(c => c.CalisanHizmetler)
                .HasForeignKey(ch => ch.CalisanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CalisanHizmet>()
                .HasOne(ch => ch.Hizmet)
                .WithMany(h => h.CalisanHizmetler)
                .HasForeignKey(ch => ch.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal precision ayarı
            modelBuilder.Entity<Hizmet>()
                .Property(h => h.Fiyat)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Randevu>()
                .Property(r => r.Fiyat)
                .HasColumnType("decimal(18,2)");

            // Randevu tablo ayarları
            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Calisan)
                .WithMany() // Eğer Calisan'ın Randevular koleksiyonu yoksa WithMany() bırakabilirsin
                .HasForeignKey(r => r.CalisanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Hizmet)
                .WithMany() // Aynı şekilde Hizmet'in Randevular koleksiyonu yoksa WithMany() diyebilirsin
                .HasForeignKey(r => r.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
