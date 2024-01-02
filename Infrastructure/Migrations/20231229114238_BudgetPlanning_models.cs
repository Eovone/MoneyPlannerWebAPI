using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BudgetPlanning_models : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SummaryAmount = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BudgetPlansItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    ReOccuring = table.Column<bool>(type: "bit", nullable: false),
                    IsIncome = table.Column<bool>(type: "bit", nullable: false),
                    BudgetPlanId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetPlansItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetPlansItems_BudgetPlans_BudgetPlanId",
                        column: x => x.BudgetPlanId,
                        principalTable: "BudgetPlans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetPlansItems_BudgetPlanId",
                table: "BudgetPlansItems",
                column: "BudgetPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetPlansItems");

            migrationBuilder.DropTable(
                name: "BudgetPlans");
        }
    }
}
