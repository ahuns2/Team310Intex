using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace testingINTEX.Models;

public partial class IntexpostgresContext : DbContext
{
    public IntexpostgresContext()
    {
    }

    public IntexpostgresContext(DbContextOptions<IntexpostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<LineItem> LineItems { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=postgresintex.postgres.database.azure.com;Port=5432;Database=intexpostgres;Username=group310;Password=WeLoveIntex!;SSL Mode=Require;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("customers_pkey");

            entity.ToTable("customers");

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Age)
                .HasPrecision(5, 2)
                .HasColumnName("age");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.CountryOfResidence)
                .HasMaxLength(100)
                .HasColumnName("country_of_residence");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .HasColumnName("gender");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
        });

        modelBuilder.Entity<LineItem>(entity =>
        {
            entity.HasKey(e => new { e.TransactionId, e.ProductId }).HasName("transactions_pkey");

            entity.ToTable("lineItems");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Qty).HasColumnName("qty");
            entity.Property(e => e.Rating).HasColumnName("rating");

            entity.HasOne(d => d.Product).WithMany(p => p.LineItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_product_id_fkey");

            entity.HasOne(d => d.Transaction).WithMany(p => p.LineItems)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_transaction_id_fkey");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.Property(e => e.TransactionId)
                .ValueGeneratedNever()
                .HasColumnName("transaction_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Bank)
                .HasMaxLength(50)
                .HasColumnName("bank");
            entity.Property(e => e.CountryOfTransaction)
                .HasMaxLength(50)
                .HasColumnName("country_of_transaction");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.DayOfWeek)
                .HasMaxLength(3)
                .HasColumnName("day_of_week");
            entity.Property(e => e.EntryMode)
                .HasMaxLength(10)
                .HasColumnName("entry_mode");
            entity.Property(e => e.Fraud).HasColumnName("fraud");
            entity.Property(e => e.ShippingAddress)
                .HasMaxLength(50)
                .HasColumnName("shipping_address");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.TypeOfCard)
                .HasMaxLength(20)
                .HasColumnName("type_of_card");
            entity.Property(e => e.TypeOfTransaction)
                .HasMaxLength(10)
                .HasColumnName("type_of_transaction");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("orders_customer_id_fkey");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("products_pkey");

            entity.ToTable("products");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.Category1).HasColumnName("category1");
            entity.Property(e => e.Category2).HasColumnName("category2");
            entity.Property(e => e.Category3).HasColumnName("category3");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImgLink).HasColumnName("img_link");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NumParts).HasColumnName("num_parts");
            entity.Property(e => e.Price)
                .HasPrecision(8, 2)
                .HasColumnName("price");
            entity.Property(e => e.PrimaryColor)
                .HasMaxLength(50)
                .HasColumnName("primary_color");
            entity.Property(e => e.SecondaryColor)
                .HasMaxLength(50)
                .HasColumnName("secondary_color");
            entity.Property(e => e.Year).HasColumnName("year");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
