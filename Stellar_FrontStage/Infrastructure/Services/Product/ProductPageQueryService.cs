using ApplicationCore.Dtos.ProductPageDto;
using ApplicationCore.Interfaces;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Product
{
    public class ProductPageQueryService : IProductPageQueryService
    {
        private readonly string _connectionString;
        private readonly IDbConnection _dbConnection;

        public ProductPageQueryService(IConfiguration configuration, IDbConnection dbConnection)
        {
            _connectionString = configuration.GetConnectionString("StellarDB") ??
             throw new ArgumentNullException("找不到連線字串");
            _dbConnection = dbConnection;
        }

        public async Task<List<GetFriendsWhoOwnThisGameInProductPageResult>> GetFriendsWhoOwnThisGameInProductPage(int currentProductId,int currentUserId)
        {
            var friendWhoOwnThisGameSql = @"
                SELECT
                    u.UserId AS Id,
                    u.UserImg AS ImgUrl,
                    u.Online,
                    u.NickName AS FriendName
                FROM [Users] AS u
                INNER JOIN [Friend] AS f
                    ON (u.UserId = f.FriendUserId OR u.UserId = f.UserId)
                INNER JOIN [ProductCollection] AS pc
                    ON u.UserId = pc.UserId
                INNER JOIN [Product] AS p
                    ON pc.ProductId = p.ProductId
                WHERE p.ProductId = @currentProductId
                AND (@currentUserId IN (f.UserId, f.FriendUserId))
                AND u.UserId != @currentUserId
                AND f.State = 1
                GROUP BY u.UserId, u.UserImg, u.Online, u.NickName";

            var friendWhoOwnThisGameResult = await _dbConnection.QueryAsync<GetFriendsWhoOwnThisGameInProductPageResult>(friendWhoOwnThisGameSql, new { currentProductId, currentUserId });
            return friendWhoOwnThisGameResult.ToList();
        }
        public async Task<List<GetTagsInProductPageResult>> GetTagsInProductPage(int currentProductId)
        {
            var tagsResultSql = @"
                SELECT 
                    t.TagId AS Id, 
                    t.TagName AS Name
                FROM TagConnect tc
                INNER JOIN Tags t ON tc.TagId = t.TagId
                WHERE tc.ProductId = @currentProductId";

            var tagsResult = await _dbConnection.QueryAsync<GetTagsInProductPageResult>(tagsResultSql, new { CurrentProductId = currentProductId });
            return tagsResult.ToList();
        }

        public async Task<List<GetRecommendProductResult>> GetRecommendProduct(List<int> productIdList)
        {
         

            var recommendResultSql = @"
                    SELECT 
                        p.ProductId AS Id,
                        p.ProductName AS Title,
                        p.ProductPrice AS Price,
                        ROUND(p.ProductPrice * IIF(pd.MaxDiscount IS NOT NULL AND GETDATE() >= pd.SalesStartDate AND GETDATE() < pd.SalesEndDate, pd.MaxDiscount, 1), 0) AS FormattedPrice,
                        p.ProductMainImageUrl AS ImgUrl,
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
                    ) AS pd ON p.ProductId = pd.ProductId
                    WHERE p.ProductId IN @productIdList AND p.ProductStatus=1
                ";

            var recommendResult = await _dbConnection.QueryAsync<GetRecommendProductResult>(recommendResultSql, new { productIdList });
            return recommendResult.ToList();
        }
    }
}
