﻿// <auto-generated />
using System;
using Area.API.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Area.API.Migrations
{
    [DbContext(typeof(AreaDbContext))]
    [Migration("20201208205904_AddRequiredToWidgetParam")]
    partial class AddRequiredToWidgetParam
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("Area.API.Models.Table.ManyToMany.UserWidgetModel", b =>
                {
                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.Property<int?>("WidgetId")
                        .HasColumnType("integer");

                    b.HasKey("UserId", "WidgetId");

                    b.HasIndex("WidgetId");

                    b.ToTable("UsersToWidgets");
                });

            modelBuilder.Entity("Area.API.Models.Table.ServiceModel", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Services");
                });

            modelBuilder.Entity("Area.API.Models.Table.UserModel", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Area.API.Models.Table.WidgetModel", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<bool?>("RequiresAuth")
                        .HasColumnType("boolean");

                    b.Property<int?>("ServiceId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ServiceId");

                    b.ToTable("Widgets");
                });

            modelBuilder.Entity("Area.API.Models.Table.ManyToMany.UserWidgetModel", b =>
                {
                    b.HasOne("Area.API.Models.Table.UserModel", "User")
                        .WithMany("Widgets")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Area.API.Models.Table.WidgetModel", "Widget")
                        .WithMany("Users")
                        .HasForeignKey("WidgetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");

                    b.Navigation("Widget");
                });

            modelBuilder.Entity("Area.API.Models.Table.UserModel", b =>
                {
                    b.OwnsMany("Area.API.Models.Table.Owned.UserServiceTokensModel", "ServiceTokens", b1 =>
                        {
                            b1.Property<int>("UserModelId")
                                .HasColumnType("integer");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer")
                                .UseIdentityByDefaultColumn();

                            b1.Property<string>("Json")
                                .HasColumnType("text");

                            b1.Property<int?>("ServiceId")
                                .HasColumnType("integer");

                            b1.HasKey("UserModelId", "Id");

                            b1.HasIndex("ServiceId");

                            b1.ToTable("UserHasServiceTokens");

                            b1.HasOne("Area.API.Models.Table.ServiceModel", "Service")
                                .WithMany()
                                .HasForeignKey("ServiceId");

                            b1.WithOwner()
                                .HasForeignKey("UserModelId");

                            b1.Navigation("Service");
                        });

                    b.OwnsMany("Area.API.Models.Table.Owned.UserWidgetParamModel", "WidgetParams", b1 =>
                        {
                            b1.Property<int>("UserModelId")
                                .HasColumnType("integer");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer")
                                .UseIdentityByDefaultColumn();

                            b1.Property<string>("Name")
                                .HasColumnType("text");

                            b1.Property<bool?>("Required")
                                .HasColumnType("boolean");

                            b1.Property<string>("Type")
                                .HasColumnType("text");

                            b1.Property<string>("Value")
                                .HasColumnType("text");

                            b1.Property<int?>("WidgetId")
                                .HasColumnType("integer");

                            b1.HasKey("UserModelId", "Id");

                            b1.HasIndex("WidgetId");

                            b1.ToTable("UserHasWidgetParams");

                            b1.WithOwner()
                                .HasForeignKey("UserModelId");

                            b1.HasOne("Area.API.Models.Table.WidgetModel", "Widget")
                                .WithMany()
                                .HasForeignKey("WidgetId");

                            b1.Navigation("Widget");
                        });

                    b.Navigation("ServiceTokens");

                    b.Navigation("WidgetParams");
                });

            modelBuilder.Entity("Area.API.Models.Table.WidgetModel", b =>
                {
                    b.HasOne("Area.API.Models.Table.ServiceModel", "Service")
                        .WithMany("Widgets")
                        .HasForeignKey("ServiceId");

                    b.OwnsMany("Area.API.Models.Table.Owned.WidgetParamModel", "Params", b1 =>
                        {
                            b1.Property<int>("WidgetModelId")
                                .HasColumnType("integer");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer")
                                .UseIdentityByDefaultColumn();

                            b1.Property<string>("Name")
                                .HasColumnType("text");

                            b1.Property<bool?>("Required")
                                .HasColumnType("boolean");

                            b1.Property<string>("Type")
                                .HasColumnType("text");

                            b1.Property<string>("Value")
                                .HasColumnType("text");

                            b1.HasKey("WidgetModelId", "Id");

                            b1.ToTable("WidgetHasParams");

                            b1.WithOwner()
                                .HasForeignKey("WidgetModelId");
                        });

                    b.Navigation("Params");

                    b.Navigation("Service");
                });

            modelBuilder.Entity("Area.API.Models.Table.ServiceModel", b =>
                {
                    b.Navigation("Widgets");
                });

            modelBuilder.Entity("Area.API.Models.Table.UserModel", b =>
                {
                    b.Navigation("Widgets");
                });

            modelBuilder.Entity("Area.API.Models.Table.WidgetModel", b =>
                {
                    b.Navigation("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
