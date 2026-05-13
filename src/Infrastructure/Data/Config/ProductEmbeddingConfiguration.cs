using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.eShopWeb.ApplicationCore.Entities;

namespace Microsoft.eShopWeb.Infrastructure.Data.Config;

public class ProductEmbeddingConfiguration : IEntityTypeConfiguration<ProductEmbedding>
{
    public void Configure(EntityTypeBuilder<ProductEmbedding> builder)
    {
        builder.ToTable("ProductEmbeddings");

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.CatalogItemId)
            .IsRequired();

        builder.Property(e => e.EmbeddingJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasIndex(e => e.CatalogItemId)
            .IsUnique();
    }
}
