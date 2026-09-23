using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bruno.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class ShortenVehicleRegistrationNumber : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "RegistrationNumber",
            table: "Vehicles",
            type: "character varying(12)",
            maxLength: 12,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(32)",
            oldMaxLength: 32);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "RegistrationNumber",
            table: "Vehicles",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(12)",
            oldMaxLength: 12);
    }
}
