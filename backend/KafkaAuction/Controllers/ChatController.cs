using KafkaAuction.Dtos;
using KafkaAuction.Models;
using KafkaAuction.Services.Interfaces;
using KafkaAuction.Utilities;
using ksqlDB.RestApi.Client.KSql.RestApi.Responses.Tables;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly IChatService _chatService;

    public ChatController(ILogger<ChatController> logger, IChatService chatService)
    {
        _logger = logger;
        _chatService = chatService;
    }

    [HttpPost("create_tables")]
    [ProducesResponseType(typeof(TablesResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTables()
    {
        var results = await _chatService.CreateChatTableAsync();

        return Ok(results);
    }

    [HttpDelete("drop_tables")]
    [ProducesResponseType(typeof(DropResourceResponseDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> DropTables()
    {
        var results = await _chatService.DropTablesAsync();

        return Ok(results);
    }

    [HttpPost("insert_message")]
    [ProducesResponseType(typeof(ChatMessageDetailedDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> InsertMessage([FromBody] ChatMessageCreatorDto chatMessageDto)
    {
        var message = new Chat_Message
        {
            Message_Id = Guid.NewGuid().ToString(),
            Auction_Id = chatMessageDto.Auction_Id,
            Username = chatMessageDto.Username,
            Message_Text = chatMessageDto.Message_Text
        };

        var (httpResponseMessage, chatMessage) = await _chatService.InsertMessageAsync(message);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            return BadRequest(httpResponseMessage.ReasonPhrase);
        }
        else
        {
            return Ok(chatMessage);
        }
    }

    [HttpPatch("update_message")]
    [ProducesResponseType(typeof(ChatMessageDetailedDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMessage([FromBody] ChatMessageUpdateDto chatMessageUpdateDto)
    {
        var (httpResponseMessage, chatMessage) = await _chatService.UpdateMessageAsync(chatMessageUpdateDto);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            return BadRequest(httpResponseMessage.ReasonPhrase);
        }
        else
        {
            return Ok(chatMessage);
        }
    }

    [HttpGet("get_all_messages")]
    [ProducesResponseType(typeof(ChatMessageDetailedDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllMessages([FromQuery] bool sortByDate = false)
    {
        var messages = await _chatService.GetAllMessages();

        if (sortByDate)
        {
            messages = Sorter.SortByDate(messages, messages => messages.Created_Timestamp!);
        }

        return Ok(messages);
    }

    [HttpGet("get_messages_for_auction")]
    [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessagesForAuction([FromQuery] string auction_Id, [FromQuery] bool sortByDate = false)
    {
        var messages = await _chatService.GetMessagesForAuction(auction_Id);

        if (sortByDate)
        {
            messages = Sorter.SortByDate(messages, messages => messages.Created_Timestamp!);
        }

        return Ok(messages);
    }

    [HttpGet("get_messages_for_auction_push_query")]
    [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessagesForAuctionPushQuery([FromQuery] string auction_Id)
    {
        var messages = await _chatService.GetMessagesForAuctionPushQuery(auction_Id);

        return Ok(messages);
    }
}