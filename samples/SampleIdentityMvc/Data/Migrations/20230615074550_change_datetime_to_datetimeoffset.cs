using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleIdentityMvc.Data.Migrations
{
    /// <inheritdoc />
    public partial class change_datetime_to_datetimeoffset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpirationTime",
                table: "ImmediateActionDatabaseModels",
                newName: "ExpirationTimeUtc");

            migrationBuilder.RenameColumn(
                name: "AddedDate",
                table: "ImmediateActionDatabaseModels",
                newName: "AddedDateUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpirationTimeUtc",
                table: "ImmediateActionDatabaseModels",
                newName: "ExpirationTime");

            migrationBuilder.RenameColumn(
                name: "AddedDateUtc",
                table: "ImmediateActionDatabaseModels",
                newName: "AddedDate");
        }
    }
}
