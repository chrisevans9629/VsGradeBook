﻿// <auto-generated />
using System;
using Grader;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Grader.Web.Migrations
{
    [DbContext(typeof(GradeBookDbContext))]
    partial class GradeBookDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity("Grader.CodeProject", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CsvCases");

                    b.Property<string>("CsvExpectedOutput");

                    b.Property<DateTimeOffset>("DateCreated");

                    b.Property<string>("Description")
                        .IsRequired();

                    b.Property<DateTimeOffset>("DueDate");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<Guid>("StudentCode");

                    b.Property<Guid>("TeacherCode");

                    b.HasKey("Id");

                    b.ToTable("CodeProjects");
                });

            modelBuilder.Entity("Grader.Submission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("DateCreated");

                    b.Property<double>("Grade");

                    b.Property<bool>("IsPlagiarized");

                    b.Property<int>("ProjectId");

                    b.Property<string>("StudentName")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("Submissions");
                });

            modelBuilder.Entity("Grader.SubmissionFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content")
                        .IsRequired();

                    b.Property<string>("FileName")
                        .IsRequired();

                    b.Property<int>("SubmissionId");

                    b.HasKey("Id");

                    b.HasIndex("SubmissionId");

                    b.ToTable("SubmissionFiles");
                });

            modelBuilder.Entity("Grader.Submission", b =>
                {
                    b.HasOne("Grader.CodeProject")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Grader.SubmissionFile", b =>
                {
                    b.HasOne("Grader.Submission")
                        .WithMany("SubmissionFiles")
                        .HasForeignKey("SubmissionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
