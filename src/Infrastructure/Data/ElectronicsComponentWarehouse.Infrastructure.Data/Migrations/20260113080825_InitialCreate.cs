using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElectronicsComponentWarehouse.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(256)", maxLength: 256, nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "varbinary(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Components",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    StorageCellNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModelNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DatasheetLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MinimumStockLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Components", x => x.Id);
                    table.CheckConstraint("CK_Component_MinimumStockLevel", "[MinimumStockLevel] >= 0");
                    table.CheckConstraint("CK_Component_StockQuantity", "[StockQuantity] >= 0");
                    table.ForeignKey(
                        name: "FK_Components_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "ParentCategoryId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4921), "Микроконтроллеры и микропроцессоры", "Microcontrollers", null, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4922) },
                    { 2, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4926), "Микросхемы и интегральные схемы", "Microchips", null, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4926) },
                    { 3, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4927), "Полевые транзисторы (MOSFET)", "Mosfets", null, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4928) },
                    { 4, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4929), "Конденсаторы", "Capacitors", null, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4929) },
                    { 5, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4930), "Резисторы", "Resistors", null, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4930) },
                    { 6, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4931), "Диоды и стабилитроны", "Diodes", null, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4932) },
                    { 7, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4932), "Биполярные транзисторы", "Transistors", null, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4933) },
                    { 8, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4934), "Разъемы и коннекторы", "Connectors", null, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4934) },
                    { 9, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4935), "Датчики и сенсоры", "Sensors", null, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4936) },
                    { 10, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4937), "Дисплеи и индикаторы", "Displays", null, new DateTime(2026, 1, 13, 8, 8, 25, 434, DateTimeKind.Utc).AddTicks(4937) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "LastLoginAt", "PasswordHash", "PasswordSalt", "Role", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 11, 14, 8, 8, 25, 435, DateTimeKind.Utc).AddTicks(3587), "admin@warehouse.local", "Администратор Системы", true, new DateTime(2026, 1, 12, 8, 8, 25, 435, DateTimeKind.Utc).AddTicks(3589), new byte[] { 65, 100, 109, 105, 110, 49, 50, 51, 33 }, new byte[] { 115, 97, 108, 116, 95, 102, 111, 114, 95, 97, 100, 109, 105, 110 }, "Admin", "admin" },
                    { 2, new DateTime(2025, 11, 14, 8, 8, 25, 435, DateTimeKind.Utc).AddTicks(3596), "user@warehouse.local", "Обычный Пользователь", true, new DateTime(2026, 1, 12, 8, 8, 25, 435, DateTimeKind.Utc).AddTicks(3597), new byte[] { 85, 115, 101, 114, 49, 50, 51, 33 }, new byte[] { 115, 97, 108, 116, 95, 102, 111, 114, 95, 117, 115, 101, 114 }, "User", "user" },
                    { 3, new DateTime(2025, 11, 14, 8, 8, 25, 435, DateTimeKind.Utc).AddTicks(3601), "manager@warehouse.local", "Менеджер Склада", true, new DateTime(2026, 1, 12, 8, 8, 25, 435, DateTimeKind.Utc).AddTicks(3601), new byte[] { 77, 97, 110, 97, 103, 101, 114, 49, 50, 51, 33 }, new byte[] { 115, 97, 108, 116, 95, 102, 111, 114, 95, 109, 97, 110, 97, 103, 101, 114 }, "User", "manager" }
                });

            migrationBuilder.InsertData(
                table: "Components",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "DatasheetLink", "Description", "LastUpdated", "Manufacturer", "MinimumStockLevel", "ModelNumber", "Name", "StockQuantity", "StorageCellNumber", "UnitPrice" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 12, 14, 8, 8, 25, 435, DateTimeKind.Utc).AddTicks(245), "https://docs.arduino.cc/resources/datasheets/A000066-datasheet.pdf", "Микроконтроллерная плата на базе ATmega328P", new DateTime(2026, 1, 12, 8, 8, 25, 435, DateTimeKind.Utc).AddTicks(253), "Arduino", 5, "A000066", "Arduino Uno R3", 25, "A-01-01", 22.00m },
                    { 2, 1, new DateTime(2025, 12, 19, 8, 8, 25, 435, DateTimeKind.Utc).AddTicks(256), "https://www.espressif.com/sites/default/files/documentation/esp32-wroom-32_datasheet_en.pdf", "Модуль WiFi+Bluetooth на ESP32", new DateTime(2026, 1, 13, 8, 8, 25, 435, DateTimeKind.Utc).AddTicks(256), "Espressif", 8, "ESP32-WROOM-32", "ESP32 DevKit", 18, "A-01-02", 8.50m },
                    { 3, 5, new DateTime(2025, 11, 14, 8, 8, 25, 435, DateTimeKind.Utc).AddTicks(259), null, "Резистор 10 кОм, 0.25 Вт, 5%", new DateTime(2026, 1, 8, 8, 8, 25, 435, DateTimeKind.Utc).AddTicks(259), "Yageo", 200, "CFR-25JB-10K", "Resistor 10kΩ 1/4W", 1000, "B-02-01", 0.02m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_CategoryId",
                table: "Components",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_Manufacturer",
                table: "Components",
                column: "Manufacturer");

            migrationBuilder.CreateIndex(
                name: "IX_Components_Name",
                table: "Components",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Components_StockQuantity",
                table: "Components",
                column: "StockQuantity");

            migrationBuilder.CreateIndex(
                name: "IX_Components_StorageCellNumber",
                table: "Components",
                column: "StorageCellNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Components");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
