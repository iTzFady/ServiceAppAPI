using System.ComponentModel.DataAnnotations;

public class ConfirmEmailDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Token { get; set; }
}