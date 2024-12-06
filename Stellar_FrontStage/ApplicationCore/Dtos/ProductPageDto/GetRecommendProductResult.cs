using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Dtos.ProductPageDto
{
    public class GetRecommendProductResult
    {
        public int Id { get; set; }
        public string ImgUrl { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public decimal FormattedPrice { get; set; }
        public decimal Discount { get; set; }
    }
}
