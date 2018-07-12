﻿// <auto-generated />
using System;
using Dwragge.RCloneClient.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dwragge.RCloneClient.Persistence.Migrations
{
    [DbContext(typeof(JobContext))]
    partial class JobContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846");

            modelBuilder.Entity("Dwragge.RCloneClient.Persistence.BackupFolderDto", b =>
                {
                    b.Property<int>("BackupFolderId")
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

                    b.HasKey("BackupFolderId");

                    b.ToTable("BackupFolders");
                });

            modelBuilder.Entity("Dwragge.RCloneClient.Persistence.FileVersionHistoryDto", b =>
                {
                    b.Property<int>("VersionHistoryId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BackupFolderId");

                    b.Property<string>("FileName")
                        .IsRequired();

                    b.Property<string>("RemoteLocation")
                        .IsRequired();

                    b.Property<DateTime>("VersionedOn");

                    b.HasKey("VersionHistoryId");

                    b.HasIndex("BackupFolderId");

                    b.ToTable("FileVersionHistory");
                });

            modelBuilder.Entity("Dwragge.RCloneClient.Persistence.InProgressFileDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BackupFolderId");

                    b.Property<string>("FileName");

                    b.Property<DateTime>("InsertedAt");

                    b.Property<string>("RemotePath");

                    b.HasKey("Id");

                    b.HasIndex("BackupFolderId");

                    b.ToTable("InProgressFiles");
                });

            modelBuilder.Entity("Dwragge.RCloneClient.Persistence.PendingFileDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BackupFolderId");

                    b.Property<string>("FileName")
                        .IsRequired();

                    b.Property<DateTime>("QueuedTime");

                    b.HasKey("Id");

                    b.HasIndex("BackupFolderId");

                    b.ToTable("PendingFiles");
                });

            modelBuilder.Entity("Dwragge.RCloneClient.Persistence.RemoteDto", b =>
                {
                    b.Property<int>("RemoteId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConnectionString")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("RemoteId");

                    b.ToTable("Remotes");
                });

            modelBuilder.Entity("Dwragge.RCloneClient.Persistence.TrackedFileDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BackupFolderId");

                    b.Property<string>("FileName")
                        .IsRequired();

                    b.Property<DateTime>("FirstBackedUp");

                    b.Property<DateTime>("LastModified");

                    b.Property<string>("RemoteLocation")
                        .IsRequired();

                    b.Property<long>("SizeBytes");

                    b.HasKey("Id");

                    b.HasIndex("BackupFolderId");

                    b.HasIndex("FileName")
                        .IsUnique();

                    b.ToTable("TrackedFiles");
                });

            modelBuilder.Entity("Dwragge.RCloneClient.Persistence.FileVersionHistoryDto", b =>
                {
                    b.HasOne("Dwragge.RCloneClient.Persistence.BackupFolderDto", "BackupFolder")
                        .WithMany()
                        .HasForeignKey("BackupFolderId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Dwragge.RCloneClient.Persistence.InProgressFileDto", b =>
                {
                    b.HasOne("Dwragge.RCloneClient.Persistence.BackupFolderDto", "BackupFolder")
                        .WithMany()
                        .HasForeignKey("BackupFolderId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Dwragge.RCloneClient.Persistence.PendingFileDto", b =>
                {
                    b.HasOne("Dwragge.RCloneClient.Persistence.BackupFolderDto", "BackupFolder")
                        .WithMany()
                        .HasForeignKey("BackupFolderId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Dwragge.RCloneClient.Persistence.TrackedFileDto", b =>
                {
                    b.HasOne("Dwragge.RCloneClient.Persistence.BackupFolderDto", "BackupFolder")
                        .WithMany()
                        .HasForeignKey("BackupFolderId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
