using Microsoft.EntityFrameworkCore.Migrations;

namespace Identity.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmployeeDesignation_Designation",
                table: "Employees");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmployeeDesignation_Designation",
                table: "Employees",
                column: "Designation",
                principalTable: "EmployeeDesignation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
