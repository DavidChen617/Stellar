using ApplicationCore.Interfaces;
using Dapper;
using Infrastructure.Data.Mongo.Entity;
using Infrastructure.Data.Mongo.Repository;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.DapperSemanticKernelRecommendProducts
{
    public class SemanticKernelRecommendProductsService
    {
        private readonly MongoRepository<RecommendProducts> _productInMongoRepository;
        private readonly string _connectionString;
        private readonly IDbConnection _dbConnection;
        private readonly IRepository<ApplicationCore.Entities.User> _userRepository;
        public SemanticKernelRecommendProductsService(MongoRepository<RecommendProducts> productInMongoRepository, IConfiguration configuration, IDbConnection dbConnection)
        {
            _productInMongoRepository = productInMongoRepository;
            _connectionString = configuration.GetConnectionString("StellarDB") ??
               throw new ArgumentNullException("找不到連線字串");
            _dbConnection = dbConnection;
        }


        public async Task<IEnumerable<RecommendProducts>> GetFetchData()
        {
            var sql = @"
                        SELECT 
                            p.ProductId,
                            p.ProductName,
                            p.ProductPrice,
                             ROUND(p.ProductPrice * IIF(pd.MaxDiscount IS NOT NULL AND GETDATE() >= pd.SalesStartDate AND GETDATE() < pd.SalesEndDate, pd.MaxDiscount, 1), 0) AS SalesPrice,
                            p.ProductMainImageUrl,
                            p.Description,
                            (1 - ISNULL(pd.MaxDiscount, 1)) * 100 AS Discount
                        FROM Product AS p
                        LEFT JOIN (
                            SELECT 
                                ProductId,
                                MAX(Discount) AS MaxDiscount,
                                SalesStartDate,
                                SalesEndDate
                            FROM ProductsDiscount
                            WHERE GETDATE() >= SalesStartDate AND GETDATE() < SalesEndDate
                            GROUP BY ProductId, SalesStartDate, SalesEndDate
                        ) AS pd ON p.ProductId = pd.ProductId;
                        ";
            var result = await _dbConnection.QueryAsync<RecommendProducts>(sql);
            return result;
        }
    }
}
