using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add temporary GUID columns for mapping
            migrationBuilder.AddColumn<Guid>(
                name: "TempId",
                table: "Users",
                type: "TEXT",
                nullable: true);

            // 2. Populate Users.TempId with random GUIDs
            // SQLite random GUID generation logic
            migrationBuilder.Sql(
                @"UPDATE Users SET TempId = lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || substr(lower(hex(randomblob(2))),2) || '-' || substr('89ab',abs(random()) % 4 + 1, 1) || substr(lower(hex(randomblob(2))),2) || '-' || lower(hex(randomblob(6)));");

            // 3. Create new Users table (Users_New)
            migrationBuilder.Sql(@"
                CREATE TABLE ""Users_New"" (
                    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_Users_New"" PRIMARY KEY,
                    ""Email"" TEXT NOT NULL,
                    ""GoogleSubjectId"" TEXT NULL,
                    ""Name"" TEXT NULL
                );
            ");

            // 4. Copy data from Users to Users_New
            migrationBuilder.Sql(@"
                INSERT INTO ""Users_New"" (""Id"", ""Email"", ""GoogleSubjectId"", ""Name"")
                SELECT ""TempId"", ""Email"", ""GoogleSubjectId"", ""Name"" FROM ""Users"";
            ");

            // 5. Create new TodoItems table (TodoItems_New)
            // Note: Referencing Users_New temporarily. When Users_New is renamed to Users, SQLite should update this reference.
            migrationBuilder.Sql(@"
                CREATE TABLE ""TodoItems_New"" (
                    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_TodoItems_New"" PRIMARY KEY,
                    ""Category"" TEXT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""Description"" TEXT NULL,
                    ""IsDone"" INTEGER NOT NULL,
                    ""MovedCounter"" INTEGER NOT NULL,
                    ""OrderIndex"" INTEGER NOT NULL,
                    ""ResolvedAt"" TEXT NULL,
                    ""Title"" TEXT NOT NULL,
                    ""UserId"" TEXT NOT NULL,
                    ""Weekday"" INTEGER NULL,
                    CONSTRAINT ""FK_TodoItems_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users_New"" (""Id"") ON DELETE CASCADE
                );
            ");

            // 6. Copy data from TodoItems to TodoItems_New, using the mapping from Users table
            migrationBuilder.Sql(@"
                INSERT INTO ""TodoItems_New"" 
                (""Id"", ""Category"", ""CreatedAt"", ""Description"", ""IsDone"", ""MovedCounter"", ""OrderIndex"", ""ResolvedAt"", ""Title"", ""UserId"", ""Weekday"")
                SELECT 
                ""Id"", ""Category"", ""CreatedAt"", ""Description"", ""IsDone"", ""MovedCounter"", ""OrderIndex"", ""ResolvedAt"", ""Title"", 
                (SELECT ""TempId"" FROM ""Users"" WHERE ""Users"".""Id"" = ""TodoItems"".""UserId""), 
                ""Weekday"" 
                FROM ""TodoItems"";
            ");

            // 7. Drop old tables
            migrationBuilder.DropTable(name: "TodoItems");
            migrationBuilder.DropTable(name: "Users");

            // 8. Rename new tables to original names
            migrationBuilder.RenameTable(name: "Users_New", schema: null, newName: "Users");
            migrationBuilder.RenameTable(name: "TodoItems_New", schema: null, newName: "TodoItems");

            // 9. Recreate Indexes
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_UserId",
                table: "TodoItems",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "TodoItems",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");
        }
    }
}
