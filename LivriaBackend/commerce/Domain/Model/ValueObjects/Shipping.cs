using System; 
using System.ComponentModel.DataAnnotations;

namespace LivriaBackend.commerce.Domain.Model.ValueObjects
{
    /// <summary>
    /// Representa un objeto de valor que contiene los detalles de envío de una orden.
    /// Un <c>record</c> es inmutable y sus propiedades se configuran solo en el momento de la construcción.
    /// </summary>
    public record Shipping
{
    [Required]
    [StringLength(255)]
    public string Address { get; init; }

    [Required]
    [StringLength(100)]
    public string City { get; init; }

    [Required]
    [StringLength(100)]
    public string District { get; init; }

    [StringLength(500)]
    public string Reference { get; init; }

    [Required]
    public decimal Price { get; init; }

    private Shipping()
    {
        Address = string.Empty;
        City = "Lima Metropolitana";
        District = string.Empty;
        Reference = string.Empty;
        Price = 0;
    }

    public Shipping(string address, string city, string district, string reference)
    {
        if (string.IsNullOrWhiteSpace(address)) throw new ArgumentNullException(nameof(address), "Address cannot be empty.");
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentNullException(nameof(city), "City cannot be empty.");
        if (string.IsNullOrWhiteSpace(district)) throw new ArgumentNullException(nameof(district), "District cannot be empty.");

        Address = address;
        City = city;
        District = district;
        Reference = reference;
        Price = CalculateShippingPrice(district); // 👈
    }

    // 👇 lógica de precio
    public static decimal CalculateShippingPrice(string district)
    {
        var zone1 = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Lince", "Pueblo Libre", "Magdalena del Mar", "San Miguel", "Breña",
            "La Victoria", "Miraflores", "San Isidro"
        };

        var zone2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Barranco", "Chorrillos", "San Borja", "Surquillo", "Santiago de Surco",
            "San Luis", "Rímac", "Independencia", "Los Olivos", "San Martín de Porres",
            "Ate", "El Agustino", "Santa Anita", "La Molina", "San Juan de Miraflores"
        };

        if (zone1.Contains(district)) return 5m;
        if (zone2.Contains(district)) return 8m;
        return 12m; // zona 3
    }
}
}