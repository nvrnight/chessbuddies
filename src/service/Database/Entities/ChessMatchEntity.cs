using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace service.Database.Entities
{
    public class ChessMatchEntityMap : IEntityTypeConfiguration<ChessMatchEntity>
    {
        public void Configure(EntityTypeBuilder<ChessMatchEntity> builder)
        {
            builder.ToTable("matches", "public");
            
            builder.Property(x => x.matchjson).IsRequired();
            builder.Property(x => x.lastmove).IsRequired(false);
             
        }
    }
    public class ChessMatchEntity
    {
        public Guid id {get; set;}
        public string matchjson {get; set;}
        public DateTime? lastmove {get; set;}
    }
}