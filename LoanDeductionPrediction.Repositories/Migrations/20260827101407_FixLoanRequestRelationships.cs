using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanDeductionPrediction.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class FixLoanRequestRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanRequests_Users_BorrowerId",
                table: "LoanRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanRequests_Users_LoanOfficerUserId",
                table: "LoanRequests");

            migrationBuilder.DropIndex(
                name: "IX_LoanRequests_LoanOfficerUserId",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "LoanOfficerUserId",
                table: "LoanRequests");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRequests_ReviewedByLoanOfficerId",
                table: "LoanRequests",
                column: "ReviewedByLoanOfficerId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanRequests_Users_BorrowerId",
                table: "LoanRequests",
                column: "BorrowerId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanRequests_Users_ReviewedByLoanOfficerId",
                table: "LoanRequests",
                column: "ReviewedByLoanOfficerId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanRequests_Users_BorrowerId",
                table: "LoanRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanRequests_Users_ReviewedByLoanOfficerId",
                table: "LoanRequests");

            migrationBuilder.DropIndex(
                name: "IX_LoanRequests_ReviewedByLoanOfficerId",
                table: "LoanRequests");

            migrationBuilder.AddColumn<int>(
                name: "LoanOfficerUserId",
                table: "LoanRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanRequests_LoanOfficerUserId",
                table: "LoanRequests",
                column: "LoanOfficerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanRequests_Users_BorrowerId",
                table: "LoanRequests",
                column: "BorrowerId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanRequests_Users_LoanOfficerUserId",
                table: "LoanRequests",
                column: "LoanOfficerUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
