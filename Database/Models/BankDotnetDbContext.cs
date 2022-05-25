using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Database.Models
{
    public partial class BankDotnetDbContext : DbContext
    {
        public BankDotnetDbContext()
        {
        }

        public BankDotnetDbContext(DbContextOptions<BankDotnetDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Balance> Balances { get; set; } = null!;
        public virtual DbSet<Bill> Bills { get; set; } = null!;
        public virtual DbSet<Credit> Credits { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Saving> Savings { get; set; } = null!;
        public virtual DbSet<Transaction> Transactions { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserRole> UserRoles { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            /*if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.

                optionsBuilder.UseSqlServer("Server=localhost;Database=BankDotnetDb;uid=sa;pwd=123Farrah#;");
            }*/
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Balance>(entity =>
            {
                entity.ToTable("Balance");

                entity.Property(e => e.AccountNumber)
                    .HasMaxLength(14)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Balances)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserToBalance");
            });

            modelBuilder.Entity<Bill>(entity =>
            {
                entity.ToTable("Bill");

                entity.Property(e => e.PaymentStatus).HasMaxLength(50);

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.VirtualAccount).HasMaxLength(50);

                entity.HasOne(d => d.Balance)
                    .WithMany(p => p.Bills)
                    .HasForeignKey(d => d.BalanceId)
                    .HasConstraintName("FK_Bill_Balance");

                entity.HasOne(d => d.Credit)
                    .WithMany(p => p.Bills)
                    .HasForeignKey(d => d.CreditId)
                    .HasConstraintName("FK_Bill_Credit");
            });

            modelBuilder.Entity<Credit>(entity =>
            {
                entity.ToTable("Credit");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DueDate).HasColumnType("datetime");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Credits)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Credit_User");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<Saving>(entity =>
            {
                entity.ToTable("Saving");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.HasOne(d => d.Balance)
                    .WithMany(p => p.Savings)
                    .HasForeignKey(d => d.BalanceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Saving_Balance");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transaction");

                entity.Property(e => e.Description).HasMaxLength(50);

                entity.Property(e => e.TransactionDate).HasColumnType("datetime");

                entity.HasOne(d => d.Bill)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.BillId)
                    .HasConstraintName("FK_Transaction_Bill");

                entity.HasOne(d => d.Credit)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.CreditId)
                    .HasConstraintName("FK_Transaction_Credit1");

                entity.HasOne(d => d.RecipientBalance)
                    .WithMany(p => p.TransactionRecipientBalances)
                    .HasForeignKey(d => d.RecipientBalanceId)
                    .HasConstraintName("FK_Transaction_Balance2");

                entity.HasOne(d => d.SenderBalance)
                    .WithMany(p => p.TransactionSenderBalances)
                    .HasForeignKey(d => d.SenderBalanceId)
                    .HasConstraintName("FK_Transaction_Balance1");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.FullName).HasMaxLength(50);

                entity.Property(e => e.Password).HasColumnType("ntext");

                entity.Property(e => e.PhoneNumber).HasMaxLength(50);

                entity.Property(e => e.Username).HasMaxLength(50);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRole");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RoleToUserRole");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserToUserRole");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
