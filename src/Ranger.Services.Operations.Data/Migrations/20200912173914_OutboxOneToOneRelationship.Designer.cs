﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Ranger.Services.Operations.Data;

namespace Ranger.Services.Operations.Data.Migrations
{
    [DbContext(typeof(OperationsDbContext))]
    [Migration("20200912173914_OutboxOneToOneRelationship")]
    partial class OutboxOneToOneRelationship
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("FriendlyName")
                        .HasColumnName("friendly_name")
                        .HasColumnType("text");

                    b.Property<string>("Xml")
                        .HasColumnName("xml")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_data_protection_keys");

                    b.ToTable("data_protection_keys");
                });

            modelBuilder.Entity("Ranger.RabbitMQ.OutboxMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("InsertedAt")
                        .HasColumnName("inserted_at")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("MessageId")
                        .HasColumnName("message_id")
                        .HasColumnType("integer");

                    b.Property<bool>("Nacked")
                        .HasColumnName("nacked")
                        .HasColumnType("boolean");

                    b.HasKey("Id")
                        .HasName("pk_outbox");

                    b.HasIndex("MessageId")
                        .IsUnique()
                        .HasName("ix_outbox_message_id");

                    b.ToTable("outbox");
                });

            modelBuilder.Entity("Ranger.RabbitMQ.RangerRabbitMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasColumnName("body")
                        .HasColumnType("text");

                    b.Property<string>("Headers")
                        .IsRequired()
                        .HasColumnName("headers")
                        .HasColumnType("text");

                    b.Property<float>("MessageVersion")
                        .HasColumnName("message_version")
                        .HasColumnType("real");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnName("type")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_ranger_rabbit_message");

                    b.ToTable("ranger_rabbit_message");
                });

            modelBuilder.Entity("Ranger.Services.Operations.Data.SagaLogData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Data")
                        .HasColumnName("data")
                        .HasColumnType("text");

                    b.Property<string>("SagaId")
                        .IsRequired()
                        .HasColumnName("saga_id")
                        .HasColumnType("text");

                    b.Property<string>("SagaType")
                        .IsRequired()
                        .HasColumnName("saga_type")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_saga_log_datas");

                    b.ToTable("saga_log_datas");
                });

            modelBuilder.Entity("Ranger.Services.Operations.Data.SagaState", b =>
                {
                    b.Property<string>("SagaId")
                        .HasColumnName("saga_id")
                        .HasColumnType("text");

                    b.Property<string>("Data")
                        .HasColumnName("data")
                        .HasColumnType("text");

                    b.Property<string>("SagaType")
                        .IsRequired()
                        .HasColumnName("saga_type")
                        .HasColumnType("text");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnName("tenant_id")
                        .HasColumnType("text");

                    b.HasKey("SagaId")
                        .HasName("pk_saga_states");

                    b.ToTable("saga_states");
                });

            modelBuilder.Entity("Ranger.RabbitMQ.OutboxMessage", b =>
                {
                    b.HasOne("Ranger.RabbitMQ.RangerRabbitMessage", "Message")
                        .WithOne("OutboxMessage")
                        .HasForeignKey("Ranger.RabbitMQ.OutboxMessage", "MessageId")
                        .HasConstraintName("fk_outbox_ranger_rabbit_message_message_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
