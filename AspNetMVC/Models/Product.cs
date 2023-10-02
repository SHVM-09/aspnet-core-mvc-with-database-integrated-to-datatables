using System.ComponentModel.DataAnnotations;
namespace AspNetMVC.Models;

public class Product
{
    public int Id { get; set; }
    [Display(Name = "Product Name")]
    [StringLength(60, MinimumLength = 3)]
    [Required]
    public string Name { get; set; }

    [Range(1, 100)]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
    [DataType(DataType.Date)]
    public DateTime ExpiryDate { get; set; }
}

