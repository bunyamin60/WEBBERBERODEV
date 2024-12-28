using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WEBBERBERODEV.Models;

namespace WEBBERBERODEV.DATA
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Calisan> Calisanlar { get; set; }
        public DbSet<CalisanHizmet> CalisanHizmetler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }

        public DbSet<CalisanCalismaSaatleri> CalisanCalismaSaatleri { get; set; }
        public DbSet<RandevuHizmet> RandevuHizmetler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Decimal precision ayarı
            modelBuilder.Entity<Hizmet>()
                .Property(h => h.Fiyat)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Randevu>()
                .Property(r => r.Fiyat)
                .HasColumnType("decimal(18,2)");

            // CalisanCalismaSaatleri ve Calisan ilişkisi
            modelBuilder.Entity<CalisanCalismaSaatleri>()
                .HasOne(ccs => ccs.Calisan)
                .WithMany(c => c.CalisanCalismaSaatleri)
                .HasForeignKey(ccs => ccs.CalisanId)
                .OnDelete(DeleteBehavior.Cascade);

            // CalisanHizmet ve Calisan ilişkisi
            modelBuilder.Entity<CalisanHizmet>()
                .HasKey(ch => new { ch.CalisanId, ch.HizmetId }); // Birleşik primary key

            modelBuilder.Entity<CalisanHizmet>()
                .HasOne(ch => ch.Calisan)
                .WithMany(c => c.CalisanHizmetler)
                .HasForeignKey(ch => ch.CalisanId)
                .OnDelete(DeleteBehavior.Cascade);

            // CalisanHizmet ve Hizmet ilişkisi
            modelBuilder.Entity<CalisanHizmet>()
                .HasOne(ch => ch.Hizmet)
                .WithMany(h => h.CalisanHizmetler)
                .HasForeignKey(ch => ch.HizmetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Randevu ve Kullanici ilişkisi
            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Kullanici)
                .WithMany()
                .HasForeignKey(r => r.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            // RandevuHizmet ilişki yapılandırması
            modelBuilder.Entity<RandevuHizmet>()
                .HasKey(rh => new { rh.RandevuId, rh.HizmetId });

            modelBuilder.Entity<RandevuHizmet>()
                .HasOne(rh => rh.Randevu)
                .WithMany(r => r.RandevuHizmetler)
                .HasForeignKey(rh => rh.RandevuId);

            modelBuilder.Entity<RandevuHizmet>()
                .HasOne(rh => rh.Hizmet)
                .WithMany()
                .HasForeignKey(rh => rh.HizmetId);

            // Calisan -> ApplicationUser ilişkisi
            modelBuilder.Entity<Calisan>()
                .HasOne(c => c.ApplicationUser)
                .WithMany() // ApplicationUser tarafında bir ICollection yoksa WithMany() diyebiliriz
                .HasForeignKey(c => c.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict); // Silinirken engellesin
        }
    }
}
