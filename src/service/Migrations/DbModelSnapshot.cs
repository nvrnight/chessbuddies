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
    partial class DbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

            modelBuilder.Entity("service.Database.Entities.GameStatEntity", b =>
                {
                    b.Property<Guid>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("challenged");

                    b.Property<long>("challenger");

                    b.Property<long>("length_seconds");

                    b.Property<long>("total_moves");

                    b.Property<long?>("winner");

                    b.HasKey("id");

                    b.ToTable("game_stats","public");
                });
#pragma warning restore 612, 618
        }
    }
}