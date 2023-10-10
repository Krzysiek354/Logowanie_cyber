using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cyber_zad_1_.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "days",
                table: "PasswordRequirements",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "days",
                table: "PasswordRequirements");
        }
    }
}
