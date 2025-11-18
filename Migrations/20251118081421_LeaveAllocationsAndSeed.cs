using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeLeaveManagement.Migrations
{
    /// <inheritdoc />
    public partial class LeaveAllocationsAndSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeLeaveAllocations",
                columns: table => new
                {
                    EmployeeLeaveAllocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "int", nullable: false),
                    TotalDays = table.Column<int>(type: "int", nullable: false),
                    UsedDays = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLeaveAllocations", x => x.EmployeeLeaveAllocationId);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveAllocations_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveAllocations_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "LeaveTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "LeaveTypes",
                columns: new[] { "LeaveTypeId", "DefaultDays", "LeaveName" },
                values: new object[,]
                {
                    { 1, 10, "Casual Leave" },
                    { 2, 10, "Annual Leave" },
                    { 3, 5, "Medical Leave" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveAllocations_EmployeeId_LeaveTypeId",
                table: "EmployeeLeaveAllocations",
                columns: new[] { "EmployeeId", "LeaveTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveAllocations_LeaveTypeId",
                table: "EmployeeLeaveAllocations",
                column: "LeaveTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeLeaveAllocations");

            migrationBuilder.DeleteData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeId",
                keyValue: 3);
        }
    }
}
