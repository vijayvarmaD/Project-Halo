using Microsoft.EntityFrameworkCore.Migrations;

namespace Identity.Migrations
{
    public partial class changedEmpTbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Department",
                table: "Employees",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Department",
                table: "Employees",
                column: "Department");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_Department",
                table: "Employees",
                column: "Department",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_Department",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_Department",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Employees");
        }
    }
}
