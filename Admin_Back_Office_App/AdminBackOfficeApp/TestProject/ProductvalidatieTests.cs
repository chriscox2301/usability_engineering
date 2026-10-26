using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;
using Xunit;

namespace TestProject
{
    public class ProductvalidatieTests
    {
        // Nieuw product waar alles goed is ingevuld
        private Product ValidProduct()
        {
            return new Product
            {    
                Naam = "Kogellager 6204",
                Omschrijving = "Diepgroefkogellager",
                Artikelnummer = "KL-6204",
                Merk = "SKF",
                Categorie = "Lagers",
                Prijs = 12.99m,
                Voorraad = 10
            };
        }

        // Controleert de regels uit Product.cs en geeft true terug als het product goed is, false als er een fout in zit.
        private bool IsValid(Product product)
        {
            var context = new ValidationContext(product);
            var mistakes = new List<ValidationResult>();
            return Validator.TryValidateObject(product, context, mistakes, true);
        }
        // Unit tests Chris ----------------------------------------------------------------------------------------------------
        // TC01
        [Fact]
        public void ValidNameAndPrice_ProductIsValid()
        {
            // Arrange
            var product = ValidProduct();
            // Act
            // Assert
            Assert.True(IsValid(product));
        }
        // TC02
        [Fact]
        public void NegativePrice_ReturnsFalse()
        {
            // Arrange
            var product = ValidProduct();
            // Act
            product.Prijs = -1.00m;
            // Assert
            Assert.False(IsValid(product));
        }

        // TC03
        [Fact]
        public void PriceZero_ReturnsFalse()
        {
            // Arrange
            var product = ValidProduct();
            // Act
            product.Prijs = 0.00m;
            // Assert
            Assert.False(IsValid(product));
        }

        // TC04
        [Fact]
        public void MaxVoorraad_IsValid()
        {
            // Arrange
            var product = ValidProduct();
            // Act
            product.Voorraad = 1000;
            // Assert
            Assert.True(IsValid(product));
        }

        // TC05
        [Fact]
        public void ToHighPrice_ReturnsError()
        {
            // Arrange
            var product = ValidProduct();
            // Act
            product.Prijs = 100000000m;
            // Assert
            Assert.False(IsValid(product));
        }

        // --------------------------------------------------------------------------------------
    }
}
