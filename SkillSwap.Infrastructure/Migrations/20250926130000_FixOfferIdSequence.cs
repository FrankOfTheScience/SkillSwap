using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSwap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixOfferIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reset the sequence to start from a higher value to avoid conflicts with seeded data
            migrationBuilder.Sql("SELECT setval('\"Offers_Id_seq\"', COALESCE((SELECT MAX(\"Id\") FROM \"Offers\"), 1), true);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No need to do anything in the down migration
        }
    }
}