﻿// <auto-generated />
using System;
using Grader;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Grader.Web.Migrations
{
    [DbContext(typeof(GradeBookDbContext))]
    [Migration("20191009002652_AddRelationships")]
    partial class AddRelationships
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Grader.CodeProject", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CsvCases");

                    b.Property<string>("CsvExpectedOutput");

                    b.Property<DateTimeOffset>("DateCreated");

                    b.Property<string>("Description");

                    b.Property<DateTimeOffset>("DueDate");

                    b.Property<string>("Name");

                    b.Property<Guid>("StudentCode");

                    b.Property<Guid>("TeacherCode");

                    b.HasKey("Id");

                    b.ToTable("CodeProjects");
                });

            modelBuilder.Entity("Grader.Submission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("DateCreated");

                    b.Property<double>("Grade");

                    b.Property<int>("ProjectId");

                    b.Property<string>("StudentName");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("Submissions");
                });

            modelBuilder.Entity("Grader.SubmissionFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Content");

                    b.Property<string>("FileName");

                    b.Property<int>("SubmissionId");

                    b.Property<int?>("SubmissionId1");

                    b.HasKey("Id");

                    b.HasIndex("SubmissionId");

                    b.HasIndex("SubmissionId1");

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
                        .WithMany()
                        .HasForeignKey("SubmissionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Grader.Submission")
                        .WithMany("SubmissionFiles")
                        .HasForeignKey("SubmissionId1");
                });
#pragma warning restore 612, 618
        }
    }
}
