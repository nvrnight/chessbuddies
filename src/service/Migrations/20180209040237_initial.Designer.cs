﻿// <auto-generated />
using ChessBuddies.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace ChessBuddies.Migrations
{
    [DbContext(typeof(Db))]
    [Migration("20180209040237_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("service.Database.Entities.ChessMatchEntity", b =>
                {
                    b.Property<Guid>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("lastmove");

                    b.Property<string>("matchjson")
                        .IsRequired();

                    b.HasKey("id");

                    b.ToTable("matches","public");
                });
#pragma warning restore 612, 618
        }
    }
}