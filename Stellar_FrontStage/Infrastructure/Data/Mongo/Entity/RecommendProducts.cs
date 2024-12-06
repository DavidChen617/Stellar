using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Mongo.Entity
{
    public class RecommendProducts
    {
        [BsonId]  // 將此屬性標記為 MongoDB 的 _id 字段
        [BsonRepresentation(BsonType.ObjectId)]  // 將 ObjectId 轉換為 string
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal? ProductPrice { get; set; }
        public decimal? SalesPrice { get; set; }
        public string? Description { get; set; }
        public string ProductMainImageUrl { get; set; }
        public decimal Discount { get; set; }
    }
}
