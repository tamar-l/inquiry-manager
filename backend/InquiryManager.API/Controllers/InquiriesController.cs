using Microsoft.AspNetCore.Mvc;
using InquiryManager.API.DTOs;
using InquiryManager.API.Models;
using InquiryManager.API.Services;

namespace InquiryManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InquiriesController(IInquiryService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<InquiryDto>>> GetInquiries([FromQuery] InquiryQueryParams queryParams)
    {
        var result = await service.GetInquiriesAsync(queryParams);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<InquirySummary>> GetSummary()
    {
        var summary = await service.GetSummaryAsync();
        return Ok(summary);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<InquiryDto>> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var (result, error) = await service.UpdateStatusAsync(id, request);

        return error switch
        {
            UpdateStatusError.NotFound      => NotFound($"Inquiry {id} not found."),
            UpdateStatusError.InvalidStatus => BadRequest($"Invalid status value: '{request.Status}'."),
            UpdateStatusError.ServerError   => StatusCode(500, "An error occurred while updating the status."),
            _                               => Ok(result)
        };
    }
}
