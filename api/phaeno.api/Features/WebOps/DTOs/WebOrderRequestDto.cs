using phaeno.api.Features.WebOps.Entities;
using System.ComponentModel.DataAnnotations;

namespace phaeno.api.Features.WebOps.DTOs
{
    public class WebOrderRequestDto
    {
        [Required]
        public required WebOrder WebOrder { get; set; }

        [Required]
        public required string RecaptchaAction { get; set; }

        [Required]
        public required string RecaptchaCode { get; set; } = "";
    }
}
