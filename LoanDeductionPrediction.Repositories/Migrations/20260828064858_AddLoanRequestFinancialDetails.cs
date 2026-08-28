using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanDeductionPrediction.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanRequestFinancialDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TenureMonths",
                table: "LoanRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CollateralDetails",
                table: "LoanRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CollateralValue",
                table: "LoanRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoanType",
                table: "LoanRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlySalary",
                table: "LoanRequests",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollateralDetails",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "CollateralValue",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "LoanType",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "MonthlySalary",
                table: "LoanRequests");

            migrationBuilder.AlterColumn<int>(
                name: "TenureMonths",
                table: "LoanRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
