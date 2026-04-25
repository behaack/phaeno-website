using System.ComponentModel.DataAnnotations;

namespace phaeno.api.Features.WebOps.Entities;

public class WebOrder
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

    [Required]
    public string Description { get; set; } = string.Empty;
}
