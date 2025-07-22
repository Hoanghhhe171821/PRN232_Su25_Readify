using System.ComponentModel.DataAnnotations;

public class CreateBookDto
{
    [Required]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    public bool IsFree { get; set; }

    [Range(0, int.MaxValue)]
    public int Price { get; set; }

    public int UnitInOrder { get; set; }

    public string? ImageUrl { get; set; }

    [Range(0.0, 1.0)]
    public decimal RoyaltyRate { get; set; }

    [Required]
    public int AuthorId { get; set; }

    public string? UploadedBy { get; set; }

    public DateTime CreateDate { get; set; } = DateTime.Now;

    public DateTime? UpdateDate { get; set; }

}
