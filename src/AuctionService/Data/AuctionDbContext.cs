using Microsoft.EntityFrameworkCore;

namespace AuctionService.data
{
    public class AuctionDbContext:DbContext
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options)
        {
        }
        
        public DbSet<Entities.Auction> Auctions { get; set; }
     

    }
}
