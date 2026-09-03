using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanDeductionPrediction.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditScoreToBorrowerLoanApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreditScore",
                table: "BorrowerLoanApplications",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditScore",
                table: "BorrowerLoanApplications");
        }
    }
}
