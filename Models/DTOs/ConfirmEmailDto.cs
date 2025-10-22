using System.ComponentModel.DataAnnotations;

public class ConfirmEmailDto
{
    [Required]
    public string Token { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }

}