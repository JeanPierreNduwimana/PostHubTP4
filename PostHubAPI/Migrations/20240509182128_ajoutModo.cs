using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostHubAPI.Migrations
{
    public partial class ajoutModo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "047beea6-38f3-43bf-b780-11b03701dd96");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "ef29bfb2-2812-40bc-8123-0bdf5ced33bc");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-1111-1111-1111-111111111111",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "870c074c-c504-4e61-9ab3-397580aa0239", "AQAAAAEAACcQAAAAEBBN0wwfFmG9ZfKVa8x+vs3FNCVnQv/ZQshdPJ+Lgp3YM2iC9mJ/tRum6jRmloWVtA==", "1273a8f5-0dbb-4891-82cb-a0759446689a" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FileName", "LockoutEnabled", "LockoutEnd", "MimeType", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "22222222-2222-2222-2222-222222222222", 0, "3b4ab345-80fb-4fe0-8b41-18b4ae1397b6", "m@m.m", false, null, false, null, null, "M@M.M", "USERMODO", "AQAAAAEAACcQAAAAEB2qyOTEyy/jDFB50IUO5S48y+gKymQoL5IpHH+1etmEUJ7e+Aho3/vu38XTaC2GoA==", null, false, "93f10b96-3172-4010-a304-7714f79cae17", false, "UserModo" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "2", "22222222-2222-2222-2222-222222222222" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "2", "22222222-2222-2222-2222-222222222222" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22222222-2222-2222-2222-222222222222");

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
    }
}
