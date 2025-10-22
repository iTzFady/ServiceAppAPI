using System.ComponentModel.DataAnnotations;

public class ReportRequestDto
{
    [Required]
    public string Report { get; set; }
}