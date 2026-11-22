using Combinations.Models.Responses;
using Combinations.Services;
using Combinations.Sessions;
using Microsoft.AspNetCore.Mvc;

namespace Combinations.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CombinationsController(
    ICombinationSession combinationSession,
    ICombinationService combinationService) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<StartResponse>> Start([FromQuery] int n, CancellationToken cancellationToken)
    {
        await using var lease = await combinationSession.GetCurrentAsync(cancellationToken);

        var total = combinationService.Start(lease.Session.State, n);
        return Ok(new StartResponse { TotalCombinations = total.ToString() });
    }

    [HttpGet("next")]
    public async Task<ActionResult<CombinationResponse>> GetNext(CancellationToken cancellationToken)
    {
        await using var lease = await combinationSession.GetCurrentAsync(cancellationToken);

        return Ok(combinationService.GetNext(lease.Session.State));
    }

    [HttpGet("all")]
    public async Task<ActionResult<CombinationsPageResponse>> GetAll(
        [FromQuery] int pageSize = 10,
        [FromQuery] long page = 1,
        CancellationToken cancellationToken = default)
    {
        await using var lease = await combinationSession.GetCurrentAsync(cancellationToken);

        return Ok(combinationService.GetAll(lease.Session.State, pageSize, page));
    }

    [HttpGet("back")]
    public async Task<ActionResult<CombinationResponse>> Back(CancellationToken cancellationToken)
    {
        await using var lease = await combinationSession.GetCurrentAsync(cancellationToken);

        return Ok(combinationService.Back(lease.Session.State));
    }

    [HttpPost("reset")]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        await using var lease = await combinationSession.GetCurrentAsync(cancellationToken);

        combinationService.Reset(lease.Session.State);
        return Ok(new { message = "State was reset." });
    }
}

