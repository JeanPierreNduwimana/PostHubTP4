using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostHubAPI.Migrations
{
    public partial class signalement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isReported",
                table: "Comments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "51dfe39e-1777-4513-a99f-0a85f11ac0f3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "6e8b7fe7-3935-4b06-acf6-100cf025e608");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9fd8c782-10de-43c5-9802-5324182dd41a", "AQAAAAEAACcQAAAAEI+Y3t9NCjdt5Tn62Eao7OzSg7jEBghBoJgGSCYVBEz7QNCHwTDHHp7ZVhT91lZsxg==", "8f88787a-e26b-4f3f-8a31-1200baf00b8c" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isReported",
                table: "Comments");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "283b3de9-d657-4217-8774-552338907dcd");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "01a02140-1433-471d-915e-0df35d2f7bff");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "cc060cd5-938b-4a10-83de-0227434b46e7", "AQAAAAEAACcQAAAAEPE4WOg769MTfVuHG0B1QVAd7kqYQQ5t1TwJcTrCUZGiDcwd1BNpb4YAH+5IgaZwKg==", "49090cbe-5e13-41eb-b90c-ced018c6bb0c" });
        }
    }
}
