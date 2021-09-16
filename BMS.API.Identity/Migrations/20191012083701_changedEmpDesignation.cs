using Microsoft.EntityFrameworkCore.Migrations;

namespace Identity.Migrations
{
    public partial class changedEmpDesignation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDesignation_Departments_Department",
                table: "EmployeeDesignation");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDesignation_Department",
                table: "EmployeeDesignation");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "EmployeeDesignation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Department",
                table: "EmployeeDesignation",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDesignation_Department",
                table: "EmployeeDesignation",
                column: "Department");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDesignation_Departments_Department",
                table: "EmployeeDesignation",
                column: "Department",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
