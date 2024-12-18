﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WEBBERBERODEV.DATA;

#nullable disable

namespace WEBBERBERODEV.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WEBBERBERODEV.Models.Calisan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Ad")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("AktifMi")
                        .HasColumnType("bit");

                    b.Property<int>("SalonId")
                        .HasColumnType("int");

                    b.Property<string>("Soyad")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Uzmanlik")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SalonId");

                    b.ToTable("Calisanlar");
                });

            modelBuilder.Entity("WEBBERBERODEV.Models.CalisanHizmet", b =>
                {
                    b.Property<int>("CalisanId")
                        .HasColumnType("int");

                    b.Property<int>("HizmetId")
                        .HasColumnType("int");

                    b.HasKey("CalisanId", "HizmetId");

                    b.HasIndex("HizmetId");

                    b.ToTable("CalisanHizmetler");
                });

            modelBuilder.Entity("WEBBERBERODEV.Models.Hizmet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Ad")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Fiyat")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("SalonId")
                        .HasColumnType("int");

                    b.Property<int>("SureDakika")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SalonId");

                    b.ToTable("Hizmetler");
                });

            modelBuilder.Entity("WEBBERBERODEV.Models.Randevu", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CalisanId")
                        .HasColumnType("int");

                    b.Property<string>("Durum")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Fiyat")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("HizmetId")
                        .HasColumnType("int");

                    b.Property<DateTime>("TarihSaat")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CalisanId");

                    b.HasIndex("HizmetId");

                    b.ToTable("Randevular");
                });

            modelBuilder.Entity("WEBBERBERODEV.Models.Salon", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<TimeSpan>("AcilisSaati")
                        .HasColumnType("time");

                    b.Property<string>("Ad")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<TimeSpan>("KapanisSaati")
                        .HasColumnType("time");

                    b.HasKey("Id");

                    b.ToTable("Salonlar");
                });

            modelBuilder.Entity("WEBBERBERODEV.Models.Calisan", b =>
                {
                    b.HasOne("WEBBERBERODEV.Models.Salon", "Salon")
                        .WithMany("Calisanlar")
                        .HasForeignKey("SalonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Salon");
                });

            modelBuilder.Entity("WEBBERBERODEV.Models.CalisanHizmet", b =>
                {
                    b.HasOne("WEBBERBERODEV.Models.Calisan", "Calisan")
                        .WithMany("CalisanHizmetler")
                        .HasForeignKey("CalisanId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("WEBBERBERODEV.Models.Hizmet", "Hizmet")
                        .WithMany("CalisanHizmetler")
                        .HasForeignKey("HizmetId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Calisan");

                    b.Navigation("Hizmet");
                });

            modelBuilder.Entity("WEBBERBERODEV.Models.Hizmet", b =>
                {
                    b.HasOne("WEBBERBERODEV.Models.Salon", "Salon")
                        .WithMany("Hizmetler")
                        .HasForeignKey("SalonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Salon");
                });

            modelBuilder.Entity("WEBBERBERODEV.Models.Randevu", b =>
                {
                    b.HasOne("WEBBERBERODEV.Models.Calisan", "Calisan")
                        .WithMany()
                        .HasForeignKey("CalisanId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("WEBBERBERODEV.Models.Hizmet", "Hizmet")
                        .WithMany()
                        .HasForeignKey("HizmetId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Calisan");

                    b.Navigation("Hizmet");
                });

            modelBuilder.Entity("WEBBERBERODEV.Models.Calisan", b =>
                {
                    b.Navigation("CalisanHizmetler");
                });

            modelBuilder.Entity("WEBBERBERODEV.Models.Hizmet", b =>
                {
                    b.Navigation("CalisanHizmetler");
                });

            modelBuilder.Entity("WEBBERBERODEV.Models.Salon", b =>
                {
                    b.Navigation("Calisanlar");

                    b.Navigation("Hizmetler");
                });
#pragma warning restore 612, 618
        }
    }
}
