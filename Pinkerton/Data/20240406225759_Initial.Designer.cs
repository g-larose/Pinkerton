﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pinkerton.Data;

#nullable disable

namespace Pinkerton.Data
{
    [DbContext(typeof(BotDbContext))]
    [Migration("20240406225759_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Pinkerton.Models.GuildMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .HasColumnType("text");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("Identifier")
                        .HasColumnType("uuid");

                    b.Property<string>("MemberId")
                        .HasColumnType("text");

                    b.Property<int?>("MemberId1")
                        .HasColumnType("integer");

                    b.Property<int?>("ServerConfigId")
                        .HasColumnType("integer");

                    b.Property<string>("ServerId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MemberId1");

                    b.HasIndex("ServerConfigId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Pinkerton.Models.Infraction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("CreatedById")
                        .HasColumnType("integer");

                    b.Property<Guid>("Identifier")
                        .HasColumnType("uuid");

                    b.Property<string>("Reason")
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.ToTable("Infraction");
                });

            modelBuilder.Entity("Pinkerton.Models.ServerConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("DefaultChannelId")
                        .HasColumnType("uuid");

                    b.Property<string[]>("FilteredWords")
                        .HasColumnType("text[]");

                    b.Property<bool>("IsFilterEnabled")
                        .HasColumnType("boolean");

                    b.Property<Guid?>("LogChannel")
                        .HasColumnType("uuid");

                    b.Property<string>("OwnerId")
                        .HasColumnType("text");

                    b.Property<string>("ServerId")
                        .HasColumnType("text");

                    b.Property<string>("ServerName")
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("Pinkerton.Models.ServerMember", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("BannedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsBanned")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("KickedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ServerId")
                        .HasColumnType("text");

                    b.Property<int>("Warnings")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("Pinkerton.Models.SystemError", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Guid>("ErrorCode")
                        .HasColumnType("uuid");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<string>("ServerId")
                        .HasColumnType("text");

                    b.Property<string>("ServerName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Errors");
                });

            modelBuilder.Entity("Pinkerton.Models.GuildMessage", b =>
                {
                    b.HasOne("Pinkerton.Models.ServerMember", "Member")
                        .WithMany()
                        .HasForeignKey("MemberId1");

                    b.HasOne("Pinkerton.Models.ServerConfig", null)
                        .WithMany("Messages")
                        .HasForeignKey("ServerConfigId");

                    b.Navigation("Member");
                });

            modelBuilder.Entity("Pinkerton.Models.Infraction", b =>
                {
                    b.HasOne("Pinkerton.Models.ServerMember", "CreatedBy")
                        .WithMany("Infraction")
                        .HasForeignKey("CreatedById");

                    b.Navigation("CreatedBy");
                });

            modelBuilder.Entity("Pinkerton.Models.ServerConfig", b =>
                {
                    b.Navigation("Messages");
                });

            modelBuilder.Entity("Pinkerton.Models.ServerMember", b =>
                {
                    b.Navigation("Infraction");
                });
#pragma warning restore 612, 618
        }
    }
}