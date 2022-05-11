using Microsoft.EntityFrameworkCore.Migrations;

namespace _04_API_HospitalAPP.Migrations
{
    public partial class hospitaldoctortables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_hopitals",
                columns: table => new
                {
                    HospitalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Img = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_hopitals", x => x.HospitalID);
                    table.ForeignKey(
                        name: "FK_tbl_hopitals_tbl_users_UserID",
                        column: x => x.UserID,
                        principalTable: "tbl_users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
               name: "tbl_doctors",
               columns: table => new
               {
                   DoctorID = table.Column<int>(type: "int", nullable: false)
                       .Annotation("SqlServer:Identity", "1, 1"),
                   Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                   Img = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   UserID = table.Column<int>(type: "int", nullable: false),
                   HospitalID = table.Column<int>(type: "int", nullable: false)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_tbl_doctors", x => x.DoctorID);
                   table.ForeignKey(
                        name: "FK_tbl_doctors_tbl_hopitals_HospitalID",
                        column: x => x.HospitalID,
                        principalTable: "tbl_hopitals",
                        principalColumn: "HospitalID",
                        onDelete: ReferentialAction.Cascade);
                   table.ForeignKey(
                        name: "FK_tbl_doctors_tbl_users_UserID",
                        column: x => x.UserID,
                        principalTable: "tbl_users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.NoAction);
               });



            migrationBuilder.CreateIndex(
                name: "IX_tbl_hopitals_UserID",
                table: "tbl_hopitals",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_doctors");

            migrationBuilder.DropTable(
                name: "tbl_hopitals");

        }
    }
}
