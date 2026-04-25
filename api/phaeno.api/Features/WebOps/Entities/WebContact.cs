using System.ComponentModel.DataAnnotations;

namespace phaeno.api.Features.WebOps.Entities
{
    public class WebContact
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(60)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(60)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string OrganizationName { get; set; } = string.Empty;

        [EmailAddress]
        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(256)]
        public string NormalizedEmail { get; set; } = string.Empty;

        public bool? SendBrochure { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
    }
}
