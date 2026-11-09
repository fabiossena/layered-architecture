using LayeredCrm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LayeredCrm.DataAccess.Configurations;

public sealed class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.ToTable("opportunities");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.Stage)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(o => o.Contact)
            .WithMany()
            .HasForeignKey(o => o.ContactId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
