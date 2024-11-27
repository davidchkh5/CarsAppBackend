using AuctionService;
using BiddingService.Models;
using Grpc.Net.Client;

namespace BiddingService.Services
{
    public class GrpcAuctionClient
    {
        private readonly ILogger<GrpcAuctionClient> _logger;
        private readonly IConfiguration _config;

        public GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration config)
        {
            _config = config;
            _logger = logger;
        }

        public Auction GetAuction(string id)
        {
            _logger.LogInformation("Calling grpc service");
            var chanel = GrpcChannel.ForAddress(_config["GrpcAuction"]);
            var client = new GrpcAuction.GrpcAuctionClient(chanel);

            var request = new GetAuctionRequest { Id = id };
            try
            {
                var reply = client.GetAuction(request);
                var auction = new Auction
                {
                    ID = reply.Auction.Id,
                    AutionEnd = DateTime.Parse(reply.Auction.AuctionEnd),
                    Seller = reply.Auction.Seller,
                    ReservePrice = reply.Auction.ReservePrice,
                };
                return auction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not call GRPC Server");
                return null;
                
            }

        }
    }
}
