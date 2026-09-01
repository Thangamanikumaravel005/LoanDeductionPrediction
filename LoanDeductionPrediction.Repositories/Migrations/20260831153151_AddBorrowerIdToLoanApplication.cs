using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanDeductionPrediction.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddBorrowerIdToLoanApplication : Migration
    {
        /// <inheritdoc />
       protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. Add BorrowerId temporarily as nullable
    migrationBuilder.AddColumn<int>(
        name: "BorrowerId",
        table: "BorrowerLoanApplications",
        type: "int",
        nullable: true);

    // 2. Fill BorrowerId using the existing Email
    migrationBuilder.Sql(@"
        UPDATE A
        SET A.BorrowerId = U.UserId
        FROM BorrowerLoanApplications A
        INNER JOIN Users U
            ON LOWER(LTRIM(RTRIM(A.Email))) =
               LOWER(LTRIM(RTRIM(U.Email)))
        WHERE A.BorrowerId IS NULL;
    ");

    // 3. Make sure every application has a BorrowerId
    migrationBuilder.Sql(@"
        IF EXISTS (
            SELECT 1
            FROM BorrowerLoanApplications
            WHERE BorrowerId IS NULL
        )
        BEGIN
            THROW 50001, 'Some loan applications could not be linked to a borrower.', 1;
        END
    ");

    // 4. Change BorrowerId from nullable to required
    migrationBuilder.AlterColumn<int>(
        name: "BorrowerId",
        table: "BorrowerLoanApplications",
        type: "int",
        nullable: false,
        oldClrType: typeof(int),
        oldType: "int",
        oldNullable: true);

    // 5. Create index
    migrationBuilder.CreateIndex(
        name: "IX_BorrowerLoanApplications_BorrowerId",
        table: "BorrowerLoanApplications",
        column: "BorrowerId");

    // 6. Create foreign key
    migrationBuilder.AddForeignKey(
        name: "FK_BorrowerLoanApplications_Users_BorrowerId",
        table: "BorrowerLoanApplications",
        column: "BorrowerId",
        principalTable: "Users",
        principalColumn: "UserId",
        onDelete: ReferentialAction.Restrict);
}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowerLoanApplications_Users_BorrowerId",
                table: "BorrowerLoanApplications");

            migrationBuilder.DropIndex(
                name: "IX_BorrowerLoanApplications_BorrowerId",
                table: "BorrowerLoanApplications");

            migrationBuilder.DropColumn(
                name: "BorrowerId",
                table: "BorrowerLoanApplications");
        }
    }
}
