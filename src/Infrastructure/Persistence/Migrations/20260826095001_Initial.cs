using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        private static readonly string[] columns = new[] { "bookCopyId", "returnedAt" };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "books",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    author = table.Column<string>(type: "character varying(300)", unicode: false, maxLength: 300, nullable: false),
                    title = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: false),
                    isbn = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    lastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    createdOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_books", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "members",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    membershipType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    firstName = table.Column<string>(type: "character varying(200)", unicode: false, maxLength: 200, nullable: false),
                    lastName = table.Column<string>(type: "character varying(200)", unicode: false, maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", unicode: false, maxLength: 320, nullable: false),
                    lastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    createdOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_members", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bookCopies",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bookId = table.Column<int>(type: "integer", nullable: false),
                    isAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    lastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    createdOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookCopies", x => x.id);
                    table.ForeignKey(
                        name: "fK_bookCopies_books_bookId",
                        column: x => x.bookId,
                        principalTable: "books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "loans",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    memberId = table.Column<int>(type: "integer", nullable: false),
                    bookCopyId = table.Column<int>(type: "integer", nullable: false),
                    borrowedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    returnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    createdOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_loans", x => x.id);
                    table.ForeignKey(
                        name: "fK_loans_members_memberId",
                        column: x => x.memberId,
                        principalTable: "members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_loans_bookCopyId",
                        column: x => x.bookCopyId,
                        principalTable: "bookCopies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "iX_bookCopies_bookId",
                table: "bookCopies",
                column: "bookId");

            migrationBuilder.CreateIndex(
                name: "iX_books_isbn",
                table: "books",
                column: "isbn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_loans_bookCopyId",
                table: "loans",
                column: "bookCopyId");

            migrationBuilder.CreateIndex(
                name: "ix_loans_bookCopyId_returnedAt",
                table: "loans",
                columns: columns);

            migrationBuilder.CreateIndex(
                name: "ix_loans_memberId",
                table: "loans",
                column: "memberId");

            migrationBuilder.CreateIndex(
                name: "iX_members_email",
                table: "members",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loans");

            migrationBuilder.DropTable(
                name: "members");

            migrationBuilder.DropTable(
                name: "bookCopies");

            migrationBuilder.DropTable(
                name: "books");
        }
    }
}
