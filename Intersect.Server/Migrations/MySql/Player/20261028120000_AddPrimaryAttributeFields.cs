using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intersect.Server.Migrations.MySql.Player
{
    /// <inheritdoc />
    public partial class AddPrimaryAttributeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrimaryAttributesJson",
                table: "Players",
                type: "longtext",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "AttributePointAllocationsJson",
                table: "Players",
                type: "longtext",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException(
                "Rolling back would discard versioned player attributes. Back up and migrate those values explicitly first."
            );
        }
    }
}
