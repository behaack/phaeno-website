using phaeno.api.Features.WebOps.Entities;
using System.ComponentModel.DataAnnotations;

namespace phaeno.api.Features.WebOps.DTOs;
public class WebContactRequestDto
{
    [Required]
    public required WebContact WebContact { get; set; }
    public required string RecaptchaAction { get; set; }
    public required string RecaptchaCode { get; set; } = "";
}
