using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Migrations;

/// <inheritdoc />
public partial class _20261121153325_AddCategories : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Category",
            table: "Expenses");

        migrationBuilder.AddColumn<int>(
            name: "CategoryId",
            table: "Expenses",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "Categories",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Categories", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Expenses_CategoryId",
            table: "Expenses",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_Categories_Name",
            table: "Categories",
            column: "Name",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Expenses_Categories_CategoryId",
            table: "Expenses",
            column: "CategoryId",
            principalTable: "Categories",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Expenses_Categories_CategoryId",
            table: "Expenses");

        migrationBuilder.DropTable(
            name: "Categories");

        migrationBuilder.DropIndex(
            name: "IX_Expenses_CategoryId",
            table: "Expenses");

        migrationBuilder.DropColumn(
            name: "CategoryId",
            table: "Expenses");

        migrationBuilder.AddColumn<string>(
            name: "Category",
            table: "Expenses",
            type: "TEXT",
            nullable: false,
            defaultValue: "");
    }
}
