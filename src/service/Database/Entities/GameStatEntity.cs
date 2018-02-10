using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace service.Database.Entities
{
    public class GameStatEntityMap : IEntityTypeConfiguration<GameStatEntity>
    {
        public void Configure(EntityTypeBuilder<GameStatEntity> builder)
        {
            builder.ToTable("game_stats", "public");

            builder.HasKey(x => x.id);

            builder.Property(x => x.winner).IsRequired(false);
            builder.Property(x => x.challenger).IsRequired();
            builder.Property(x => x.challenged).IsRequired();
            builder.Property(x => x.length_seconds).IsRequired();
            builder.Property(x => x.total_moves).IsRequired();

        }
    }
    public class GameStatEntity
    {
        public Guid id {get; set;} = Guid.NewGuid();
        public long? winner {get; set;}
        public long challenger {get; set;}
        public long challenged {get; set;}
        public long length_seconds {get; set;}
        public long total_moves {get; set;}
    }
}