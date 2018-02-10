using System;
using ChessBuddies.Chess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using service.Database.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace ChessBuddies.Database
{
    public class Db : DbContext
    {
        public Db() {}
        public Db(DbContextOptions<Db> options) : base(options)
        {
            
        }

        public virtual DbSet<ChessMatchEntity> Matches {get; set;}
        public virtual DbSet<GameStatEntity> GameStats {get; set;} 

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = Program.GetConfiguration();
            optionsBuilder.UseNpgsql(configuration["Db"]);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ChessMatchEntityMap());
            modelBuilder.ApplyConfiguration(new GameStatEntityMap());
        }
        public async Task EndMatch(ChessMatch match, long? winner)
        {
            await Task.Run(async () => {
                var matchEntity = await Matches.SingleAsync(x => x.id == match.Id);
                await GameStats.AddAsync(new GameStatEntity {
                    winner = winner,
                    challenged = (long)match.Challenged,
                    challenger = (long)match.Challenger,
                    total_moves = match.History.Count(),
                    length_seconds = (long)(DateTime.UtcNow - match.CreatedDate).TotalSeconds
                });
                Matches.Remove(matchEntity);
            });
        }
        public async Task SaveOrUpdateMatch(ChessMatch match)
        {
            await Task.Run(async () => {
                var matchEntity = await Matches.SingleOrDefaultAsync(x => x.id == match.Id);
                if(matchEntity == null)
                {
                    matchEntity = new ChessMatchEntity { id = match.Id };
                    Matches.Add(matchEntity);
                }
                matchEntity.matchjson = JsonConvert.SerializeObject(match);
                if(match.History.Any())
                {
                    matchEntity.lastmove = match.History.OrderByDescending(x => x.MoveDate).First().MoveDate;
                }
            });
        }
    }
}