﻿// <auto-generated />
using System;
using EricaChats.DataAccess.Services.SQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EricaChats.DataAccess.Migrations
{
    [DbContext(typeof(EricaChats_DBContext))]
    partial class EricaChats_DBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-preview2-35157")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("EricaChats.DataAccess.Models.Channel", b =>
                {
                    b.Property<long>("ChannelID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ChannelName");

                    b.HasKey("ChannelID");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("EricaChats.DataAccess.Models.ChatMessage", b =>
                {
                    b.Property<long>("ChatMessageID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("ChannelID");

                    b.Property<string>("ChatMessageBody");

                    b.Property<DateTime>("CreatedDateTime");

                    b.Property<string>("FileAttachmentGUID");

                    b.Property<string>("FriendlyFileName");

                    b.Property<DateTime>("ModifiedDateTime");

                    b.Property<string>("SenderUserName");

                    b.HasKey("ChatMessageID");

                    b.HasIndex("ChannelID");

                    b.ToTable("ChatMessages");
                });

            modelBuilder.Entity("EricaChats.DataAccess.Models.ChatMessage", b =>
                {
                    b.HasOne("EricaChats.DataAccess.Models.Channel", "Channel")
                        .WithMany("ChatMessages")
                        .HasForeignKey("ChannelID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
