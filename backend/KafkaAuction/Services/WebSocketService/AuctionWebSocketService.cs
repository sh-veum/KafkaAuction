using System.Net.WebSockets;
using System.Text;
using KafkaAuction.Dtos;
using KafkaAuction.Models;
using KafkaAuction.Services.Interfaces.WebSocketService;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using Newtonsoft.Json;

namespace KafkaAuction.Services.WebSocketService;

public class AuctionWebSocketService : IAuctionWebSocketService
{
    private readonly ILogger<AuctionWebSocketService> _logger;
    private readonly KSqlDBContext _context;

    public AuctionWebSocketService(ILogger<AuctionWebSocketService> logger, IConfiguration configuration)
    {
        _logger = logger;

        var _ksqlDbUrl = configuration.GetValue<string>("KSqlDb:Url") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_ksqlDbUrl))
        {
            throw new InvalidOperationException("KSqlDb:Url configuration is missing");
        }

        var contextOptions = new KSqlDBContextOptions(_ksqlDbUrl)
        {
            ShouldPluralizeFromItemName = true
        };

        _context = new KSqlDBContext(contextOptions);
    }

    /// <summary>
    /// Subscribes the provided WebSocket to updates for the specified auction.
    /// </summary>
    /// <param name="webSocket">The WebSocket to send auction updates to.</param>
    /// <param name="auctionId">The ID of the auction to subscribe to.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task SubscribeToAuctionBidUpdatesAsync(WebSocket webSocket, string auctionId)
    {
        _logger.LogInformation($"Subscribing to WebSocket for auctionId: {auctionId}");

        var subscription = _context.CreatePushQuery<Auction_Bid>()
            .WithOffsetResetPolicy(AutoOffsetReset.Latest)
            .Where(p => p.Auction_Id == auctionId)
            .Select(l => new AuctionBidMessageDto
            {
                Username = l.Username,
                Bid_Amount = l.Bid_Amount,
                Timestamp = l.Timestamp
            })
            .Subscribe(AuctionBidDto =>
            {
                var message = JsonConvert.SerializeObject(AuctionBidDto);
                var buffer = Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(buffer);

                _logger.LogInformation($"Sending message from auction bid socket: {message}");

                webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            },
            error => _logger.LogError(error, "Error in SubscribeToAuctionBidUpdatesAsync WebSocket subscription"));

        _logger.LogInformation("SubscribeToAuctionBidUpdatesAsync WebSocket subscription completed");

        // Keep the WebSocket open until closed by the client
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result;
        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        } while (!result.CloseStatus.HasValue);

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    public async Task SubscribeToAuctionOverviewUpdatesAsync(WebSocket webSocket)
    {
        _logger.LogInformation($"Subscribing to WebSocket for auction updates");

        var subscription = _context.CreatePushQuery<Auction>()
            .WithOffsetResetPolicy(AutoOffsetReset.Latest)
            .Where(p => p.Is_Existing == true)
            .Select(l => new AuctionDto
            {
                Auction_Id = l.Auction_Id,
                Title = l.Title,
                Description = l.Description,
                Number_Of_Bids = l.Number_Of_Bids,
                Starting_Price = l.Starting_Price,
                Current_Price = l.Current_Price,
                Leader = l.Leader,
                Winner = l.Winner,
                Created_At = l.Created_At,
                End_Date = l.End_Date,
                Is_Open = l.Is_Open
            })
            .Subscribe(AuctionDto =>
            {
                var message = JsonConvert.SerializeObject(AuctionDto);
                var buffer = Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(buffer);

                _logger.LogInformation($"Sending message from auction overview socket: {message}");

                webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            },
            error => _logger.LogError(error, "Error in SubscribeToAuctionOverviewUpdatesAsync WebSocket subscription"));

        _logger.LogInformation("SubscribeToAuctionOverviewUpdatesAsync WebSocket subscription completed");

        // Keep the WebSocket open until closed by the client
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result;
        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        } while (!result.CloseStatus.HasValue);

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    public async Task SubscribeToAllRecentBidsAsync(WebSocket webSocket)
    {
        _logger.LogInformation("Subscribing to WebSocket for all recent bids");

        var subscription = _context.CreatePushQuery<Auction_With_Bids>("AUCTIONS_WITH_BIDS")
            .WithOffsetResetPolicy(AutoOffsetReset.Latest)
            .Select(l => new AuctionWithBidDto
            {
                Title = l.Title,
                Username = l.Username,
                Bid_Amount = l.Bid_Amount,
                Timestamp = l.Timestamp
            })
            .Subscribe(AuctionWithBids =>
            {
                var message = JsonConvert.SerializeObject(AuctionWithBids);
                var buffer = Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(buffer);

                var socketId = webSocket.GetHashCode();
                _logger.LogInformation($"Sending message from socket {socketId}: {message}");

                webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            },
            error => _logger.LogError(error, "Error in SubscribeToAllRecentBidsAsync WebSocket subscription"));

        _logger.LogInformation("SubscribeToAllRecentBidsAsync WebSocket subscription completed");

        // Keep the WebSocket open until closed by the client
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result;
        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        } while (!result.CloseStatus.HasValue);

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
}
