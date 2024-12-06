using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Linebot.SemanticRecommendProducts.Dtos
{
    public class RecommendProductsResult
    {
        //public int ProductId { get; set; }
        //public string ProductName { get; set; }
        //public decimal ProductPrice { get; set; }
        //public string ProductMainImageUrl { get; set; }
        //public string Description { get; set; }
        //public ProductsDiscount ProductsDiscount { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Relevance { get; set; }
    }

}
//public class ProductsDiscount
//{
//    public int ProductId { get; set; }

//    public decimal Discount { get; set; }

//}
