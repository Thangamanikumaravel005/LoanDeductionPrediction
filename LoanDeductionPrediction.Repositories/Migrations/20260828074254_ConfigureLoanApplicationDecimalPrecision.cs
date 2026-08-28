using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanDeductionPrediction.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureLoanApplicationDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CollateralValue",
                table: "BorrowerLoanApplications",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollateralValue",
                table: "BorrowerLoanApplications");
        }
    }
}
