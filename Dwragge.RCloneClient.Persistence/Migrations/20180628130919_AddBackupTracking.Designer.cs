﻿// <auto-generated />
using System;
using Dwragge.RCloneClient.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dwragge.RCloneClient.Persistence.Migrations
{
    [DbContext(typeof(JobContext))]
    [Migration("20180628130919_AddBackupTracking")]
    partial class AddBackupTracking
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846");

            modelBuilder.Entity("Dwragge.RCloneClient.Persistence.BackedUpFileDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FileName")
                        .IsRequired();

                    b.Property<DateTime>("FirstBackedUp");

                    b.Property<bool>("IsArchived");

                    b.Property<DateTime>("LastModified");

                    b.Property<string>("ParentFolder")
                        .IsRequired();

                    b.Property<string>("RemoteLocation")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("FileName");

                    b.HasIndex("FileName", "RemoteLocation")
                        .IsUnique();

                    b.ToTable("BackedUpFiles");
                });

            modelBuilder.Entity("Dwragge.RCloneClient.Persistence.BackupFolderDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("LastSync");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Path")
                        .IsRequired();

                    b.Property<bool>("RealTimeUpdates");

                    b.Property<string>("RemoteBaseFolder");

                    b.Property<string>("RemoteName")
                        .IsRequired();

                    b.Property<int>("SyncTimeHour");

                    b.Property<int>("SyncTimeMinute");

                    b.Property<TimeSpan>("SyncTimeSpan");

                    b.HasKey("Id");

                    b.ToTable("BackupFolders");
                });
#pragma warning restore 612, 618
        }
    }
}
