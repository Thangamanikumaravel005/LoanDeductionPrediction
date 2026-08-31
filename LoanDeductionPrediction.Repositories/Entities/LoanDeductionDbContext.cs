using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.Repositories.Entities
{
    public partial class LoanDeductionDbContext : DbContext
    {
        public LoanDeductionDbContext(
            DbContextOptions<LoanDeductionDbContext> options)
            : base(options)
        {
        }

        // ============================================================
        // DbSets
        // ============================================================

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<LoanAccount> LoanAccounts { get; set; }

        public virtual DbSet<RepaymentSchedule> RepaymentSchedules { get; set; }

        public virtual DbSet<PaymentBehaviorLog> PaymentBehaviorLogs { get; set; }

        public virtual DbSet<RiskPrediction> RiskPredictions { get; set; }

        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

        public virtual DbSet<Payment> Payments { get; set; }

        public virtual DbSet<BorrowerLoanApplication>
            BorrowerLoanApplications { get; set; }

        public virtual DbSet<LoanHistory> LoanHistories { get; set; }


        // ============================================================
        // Model Configuration
        // ============================================================

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // ========================================================
            // LOAN HISTORY
            // ========================================================

            modelBuilder.Entity<LoanHistory>(entity =>
            {
                entity.HasKey(e => e.LoanHistoryId);

                entity.Property(e => e.PrincipalAmount)
                    .HasPrecision(18, 2);

                entity.Property(e => e.InterestRate)
                    .HasPrecision(18, 2);

                entity.Property(e => e.EmiAmount)
                    .HasPrecision(18, 2);

                entity.Property(e => e.OutstandingAmount)
                    .HasPrecision(18, 2);

                entity.Property(e => e.Status)
                    .HasMaxLength(50);

                entity.HasIndex(e => e.OriginalLoanId);
            });


            // ========================================================
            // PAYMENT
            // ========================================================

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);

                entity.ToTable("Payments");

                entity.Property(e => e.PaymentId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.PaymentDate)
                    .IsRequired();

                entity.Property(e => e.PaymentStatus)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .IsRequired();


                // Payment -> RepaymentSchedule

                entity.HasOne(e => e.Schedule)
                    .WithMany()
                    .HasForeignKey(e => e.ScheduleId)
                    .OnDelete(DeleteBehavior.Restrict);


                // Payment -> LoanAccount

                entity.HasOne(e => e.Loan)
                    .WithMany()
                    .HasForeignKey(e => e.LoanId)
                    .OnDelete(DeleteBehavior.Restrict);


                // Payment -> Borrower/User

                entity.HasOne(e => e.Borrower)
                    .WithMany()
                    .HasForeignKey(e => e.BorrowerId)
                    .OnDelete(DeleteBehavior.Restrict);


                entity.HasIndex(e => e.BorrowerId);

                entity.HasIndex(e => e.LoanId);

                entity.HasIndex(e => e.ScheduleId);
            });


            // ========================================================
            // USER
            // ========================================================

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("Users");

                entity.Property(e => e.UserId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.FullName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Role)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .IsRequired();
            });


            // ========================================================
            // LOAN ACCOUNT
            // ========================================================

            modelBuilder.Entity<LoanAccount>(entity =>
            {
                entity.HasKey(e => e.LoanId);

                entity.ToTable("LoanAccounts");

                entity.Property(e => e.LoanId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.PrincipalAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.InterestRate)
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();

                entity.Property(e => e.TenureMonths)
                    .IsRequired();

                entity.Property(e => e.Emiamount)
                    .HasColumnName("EMIAmount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.OutstandingAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(e => e.StartDate)
                    .IsRequired();

                entity.Property(e => e.EndDate)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .IsRequired();


                // Borrower relationship

                entity.HasOne(e => e.Borrower)
                    .WithMany(e => e.LoanAccountBorrowers)
                    .HasForeignKey(e => e.BorrowerId)
                    .OnDelete(DeleteBehavior.NoAction);


                // Loan Officer relationship

                entity.HasOne(e => e.LoanOfficer)
                    .WithMany(e => e.LoanAccountLoanOfficers)
                    .HasForeignKey(e => e.LoanOfficerId)
                    .OnDelete(DeleteBehavior.NoAction);


                entity.HasIndex(e => e.BorrowerId);

                entity.HasIndex(e => e.LoanOfficerId);
            });


            // ========================================================
            // REPAYMENT SCHEDULE
            // ========================================================

            modelBuilder.Entity<RepaymentSchedule>(entity =>
            {
                entity.HasKey(e => e.ScheduleId);

                entity.ToTable("RepaymentSchedules");

                entity.Property(e => e.ScheduleId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.InstallmentNumber)
                    .IsRequired();

                entity.Property(e => e.DueDate)
                    .IsRequired();

                entity.Property(e => e.PrincipalAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.InterestAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Emiamount)
                    .HasColumnName("EMIAmount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.PaidAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.PaidDate)
                    .IsRequired(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(30)
                    .IsRequired();


                entity.HasOne(e => e.Loan)
                    .WithMany(e => e.RepaymentSchedules)
                    .HasForeignKey(e => e.LoanId)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.HasIndex(e => e.LoanId);
            });


            // ========================================================
            // PAYMENT BEHAVIOR LOG
            // ========================================================

            modelBuilder.Entity<PaymentBehaviorLog>(entity =>
            {
                entity.HasKey(e => e.BehaviorLogId);

                entity.ToTable("PaymentBehaviorLogs");

                entity.Property(e => e.BehaviorLogId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.DueDate)
                    .IsRequired();

                entity.Property(e => e.PaymentDate)
                    .IsRequired(false);

                entity.Property(e => e.DaysLate)
                    .IsRequired();

                entity.Property(e => e.PaymentStatus)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(e => e.RecordedAt)
                    .HasDefaultValueSql("(getdate())")
                    .IsRequired();


                entity.HasOne(e => e.Borrower)
                    .WithMany(e => e.PaymentBehaviorLogs)
                    .HasForeignKey(e => e.BorrowerId)
                    .OnDelete(DeleteBehavior.NoAction);


                entity.HasOne(e => e.Loan)
                    .WithMany(e => e.PaymentBehaviorLogs)
                    .HasForeignKey(e => e.LoanId)
                    .OnDelete(DeleteBehavior.NoAction);


                entity.HasOne(e => e.Schedule)
                    .WithMany(e => e.PaymentBehaviorLogs)
                    .HasForeignKey(e => e.ScheduleId)
                    .OnDelete(DeleteBehavior.NoAction);


                entity.HasIndex(e => e.BorrowerId);

                entity.HasIndex(e => e.LoanId);

                entity.HasIndex(e => e.ScheduleId);
            });


            // ========================================================
            // RISK PREDICTION
            // ========================================================

            modelBuilder.Entity<RiskPrediction>(entity =>
            {
                entity.HasKey(e => e.RiskPredictionId);

                entity.ToTable("RiskPredictions");

                entity.Property(e => e.RiskPredictionId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.RiskScore)
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();

                entity.Property(e => e.RiskLevel)
                    .HasMaxLength(30)
                    .IsRequired();


                entity.HasOne(e => e.Borrower)
                    .WithMany(e => e.RiskPredictions)
                    .HasForeignKey(e => e.BorrowerId)
                    .OnDelete(DeleteBehavior.NoAction);


                entity.HasOne(e => e.Loan)
                    .WithMany(e => e.RiskPredictions)
                    .HasForeignKey(e => e.LoanId)
                    .OnDelete(DeleteBehavior.NoAction);


                entity.HasIndex(e => e.BorrowerId);

                entity.HasIndex(e => e.LoanId);
            });


            // ========================================================
            // REFRESH TOKEN
            // ========================================================

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.RefreshTokenId);

                entity.ToTable("RefreshTokens");

                entity.Property(e => e.RefreshTokenId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.Token)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.ExpiresAt)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.RevokedAt)
                    .IsRequired(false);

                entity.Property(e => e.IsRevoked)
                    .IsRequired()
                    .ValueGeneratedNever();


                // User relationship

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .HasPrincipalKey(u => u.UserId)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.HasIndex(e => e.Token)
                    .IsUnique();

                entity.HasIndex(e => e.UserId);
            });


            // ========================================================
            // BORROWER LOAN APPLICATION
            // ========================================================

            modelBuilder.Entity<BorrowerLoanApplication>(entity =>
            {
                entity.HasKey(e => e.ApplicationId);

                entity.ToTable("BorrowerLoanApplications");

                entity.Property(e => e.ApplicationId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.FullName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.DateOfBirth)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.MonthlySalary)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired(false);

                entity.Property(e => e.CollateralDetails)
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.Property(e => e.LoanType)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.RequestedAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(e => e.InterestRate)
                    .HasColumnType("decimal(5,2)")
                    .IsRequired(false);

                entity.Property(e => e.TenureMonths)
                    .IsRequired(false);

                entity.Property(e => e.Remarks)
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .IsRequired();

                entity.Property(e => e.ReviewedAt)
                    .IsRequired(false);


                entity.HasOne(e => e.ReviewedByLoanOfficer)
                    .WithMany(u => u.ReviewedBorrowerApplications)
                    .HasForeignKey(e => e.ReviewedByLoanOfficerId)
                    .OnDelete(DeleteBehavior.Restrict);


                entity.HasIndex(e => e.Email);

                entity.HasIndex(e => e.Status);
            });
        }
    }
}