using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using phaeno.api.Features.WebOps.DTOs;
using phaeno.api.Features.WebOps.Services;
using phaeno.api.Infrastructure.WebSearchServices;

namespace phaeno.api.Features.WebOps.Controllers;

/// <summary>
/// Public web operations supporting the Phaeno marketing website.
/// 
/// These endpoints are intended for anonymous, public-facing use
/// (search, contact forms, and non-binding order inquiries).
/// </summary>
[EnableCors("Website")]
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/web-ops")]
//[Tags("Public Web")]
public class WebOpsController(
    WebOpsService webOpsService,
    IWebSearchService webSearchService
) : ControllerBase
{
    /// <summary>
    /// Q public website pages.
    /// </summary>
    /// <remarks>
    /// Performs a lightweight full-text search across public-facing website
    /// content such as marketing pages, documentation, and announcements.
    /// 
    /// This endpoint is designed to support client-side search experiences
    /// on the public Phaeno website.
    /// </remarks>
    /// <param name="search">Free-text search query.</param>
    /// <returns>List of matching public pages.</returns>
    /// <response code="200">Q results returned successfully.</response>
    /// <response code="400">Q query was missing or invalid.</response>
    [HttpGet("search-pages")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Search(
        [FromQuery] string search)
    {
        var results = webSearchService.Search(search);
        return Ok(results);
    }

    /// <summary>
    /// Submit a public contact request.
    /// </summary>
    /// <remarks>
    /// Accepts inbound contact requests from the public website
    /// (e.g. Contact Us, partnership inquiries, general questions).
    /// 
    /// This endpoint does not create user accounts and does not
    /// require authentication. Submissions are reviewed internally
    /// by the Phaeno team.
    /// </remarks>
    /// <param name="dto">Contact request details.</param>
    /// <response code="204">Contact request accepted.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="429">Rate limit exceeded.</response>
    [HttpPost("contact")]
    [AllowAnonymous]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Contact(
        [FromBody] WebContactRequestDto dto)
    {
        await webOpsService.CreateContact(dto);
        return NoContent();
    }

    /// <summary>
    /// Submit a preliminary product or service order inquiry.
    /// </summary>
    /// <remarks>
    /// Captures non-binding expressions of interest for Phaeno products
    /// and services (e.g. PSeq RUO service, pilot studies, kits).
    /// 
    /// This endpoint does not process payments and does not create
    /// contractual obligations. Submissions are followed up by
    /// the Phaeno commercial team.
    /// </remarks>
    /// <param name="dto">Order inquiry details.</param>
    /// <response code="204">Order inquiry accepted.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="429">Rate limit exceeded.</response>
    [HttpPost("order")]
    [AllowAnonymous]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Order(
        [FromBody] WebOrderRequestDto dto)
    {
        await webOpsService.CreateOrder(dto);
        return NoContent();
    }
}
