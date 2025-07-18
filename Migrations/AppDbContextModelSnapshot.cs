﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ServiceApp.Data;

#nullable disable

namespace ServiceApp.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ServiceApp.Models.Rating", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Comment")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("RatedByUserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RatedUserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ServiceRequestId")
                        .HasColumnType("uuid");

                    b.Property<int>("Stars")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("RatedByUserId");

                    b.HasIndex("RatedUserId");

                    b.HasIndex("ServiceRequestId");

                    b.ToTable("Ratings");
                });

            modelBuilder.Entity("ServiceApp.Models.Report", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ReportedByUserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ReportedUserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ReportedByUserId");

                    b.HasIndex("ReportedUserId");

                    b.ToTable("Reports");
                });

            modelBuilder.Entity("ServiceApp.Models.ServiceRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("CompletedTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ImageUrls")
                        .HasColumnType("text");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("RequestedByUserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RequestedForUserId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("RequestedTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("SpecialtyRequired")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("RequestedByUserId");

                    b.HasIndex("RequestedForUserId");

                    b.ToTable("ServiceRequests");
                });

            modelBuilder.Entity("ServiceApp.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("EmailConfirmationToken")
                        .HasColumnType("text");

                    b.Property<DateTime?>("EmailConfirmationTokenExpiry")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool?>("IsAvailable")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsBanned")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NationalNumber")
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PasswordResetToken")
                        .HasColumnType("text");

                    b.Property<DateTime?>("PasswordResetTokenExpiry")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("WorkerSpecialty")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("NationalNumber")
                        .IsUnique();

                    b.HasIndex("PhoneNumber")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ServiceApp.Models.Rating", b =>
                {
                    b.HasOne("ServiceApp.Models.User", "RatedBy")
                        .WithMany()
                        .HasForeignKey("RatedByUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ServiceApp.Models.User", "RatedUser")
                        .WithMany()
                        .HasForeignKey("RatedUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ServiceApp.Models.ServiceRequest", "serviceRequest")
                        .WithMany()
                        .HasForeignKey("ServiceRequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RatedBy");

                    b.Navigation("RatedUser");

                    b.Navigation("serviceRequest");
                });

            modelBuilder.Entity("ServiceApp.Models.Report", b =>
                {
                    b.HasOne("ServiceApp.Models.User", "ReportedByUser")
                        .WithMany()
                        .HasForeignKey("ReportedByUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ServiceApp.Models.User", "ReportedUser")
                        .WithMany()
                        .HasForeignKey("ReportedUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ReportedByUser");

                    b.Navigation("ReportedUser");
                });

            modelBuilder.Entity("ServiceApp.Models.ServiceRequest", b =>
                {
                    b.HasOne("ServiceApp.Models.User", "RequestedBy")
                        .WithMany()
                        .HasForeignKey("RequestedByUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ServiceApp.Models.User", "RequestedFor")
                        .WithMany()
                        .HasForeignKey("RequestedForUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RequestedBy");

                    b.Navigation("RequestedFor");
                });
#pragma warning restore 612, 618
        }
    }
}
