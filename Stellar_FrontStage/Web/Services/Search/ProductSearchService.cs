using ApplicationCore.Interfaces;
using AutoMapper;
using Humanizer;
using Infrastructure.Data.Mongo.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver.Linq;
using System.Data;
using System.Linq;
using Web.Extensions;
using Web.Helpers;
using Web.ViewModels.Product;

namespace Web.Services.Search
{
    public class ProductSearchServices
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductComment> _productCommentRepository;
        private readonly IRepository<ApplicationCore.Entities.Tag> _tagRepository;
        private readonly IRepository<ApplicationCore.Entities.Category> _categoryRepository;
        private readonly IRepository<ProductsDiscount> _productsDiscountRepository;
        private readonly IRepository<TagConnect> _tagConnectRepository;
        private readonly IRepository<ProductCollection> _productCollectionRepository;
        private readonly List<TagVM> _tags;

        private readonly List<CategoryVM> _categorys;
        private readonly IDbConnection _dbConnection;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductSearchServices> _logger;

        private readonly IDistributedCache _distributedCache; //快取的東西
        public ProductSearchServices(

         
            IRepository<Product> productRepository,
            IRepository<ProductComment> productCommentRepository,
            IRepository<ApplicationCore.Entities.Tag> tagRepository,
            IRepository<ProductsDiscount> ProductsDiscountRepository,
            IRepository<ApplicationCore.Entities.Category> categoryRepository,
            IRepository<TagConnect> tagConnectRepository,
            IRepository<ProductCollection> productCollectionRepository,
            IDistributedCache distributedCache,
            IMapper mapper,
            ILogger<ProductSearchServices> logger,
            IDbConnection dbConnection
            )
        {
            _productRepository = productRepository;
            _productCommentRepository = productCommentRepository;
            _tagRepository = tagRepository;
            _productsDiscountRepository = ProductsDiscountRepository;
            _categoryRepository = categoryRepository;
            _tagConnectRepository = tagConnectRepository;
            _productCollectionRepository = productCollectionRepository;


            //通過建構函式 注入 AutoMapper 與 logger
            _mapper = mapper;
            _logger = logger;

            // 使用 AutoMapper 將數據庫中的類別和標籤轉換為前端的 ViewModel
            _categorys = GetItems<Category, CategoryVM>(categoryRepository);
            _tags = GetItems<ApplicationCore.Entities.Tag, TagVM>(tagRepository);

            _distributedCache = distributedCache;
            _dbConnection = dbConnection;
        }



        private List<TVM> GetItems<T, TVM>(IRepository<T> repository) where T : class where TVM : class
        {
            try
            {
                // 從數據庫獲取所有項目
                var items = repository.List();

                if (items == null || !items.Any())
                {
                    return new List<TVM>();
                }
                // 使用 AutoMapper 將查詢到的數據映射為 ViewModel 的列表
                return _mapper.Map<List<TVM>>(items);
            }
            catch (Exception ex)
            {
                // 記錄映射過程中出現的異常，並且拋出異常以供上層捕獲處理
                _logger.LogError(ex, "Error mapping items from {Repository}", repository.GetType().Name);
                throw;
            }
        }




        TimeSpan slidingExpiration = TimeSpan.FromMinutes(1);
        TimeSpan absoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

        string cacheKeyRedisStr()
        {
            return "ProductSearchViewModel-Redis";
        }



        private ProductSearchVM CreateProductSearchViewModel(List<ProductInfoVM> products, List<SearchTagVM> selectedTags = null, List<SearchCategoryVM> selectedCategories = null, int count = 0)
        {
            return new ProductSearchVM
            {
                Categorys = _categorys,
                Tags = _tags,
                searchCategorys = selectedCategories,
                searchTags = selectedTags,
                Products = products,
                TotalCount = count == 0 ? products.Count() : count,
            };
        }



        private async Task<IEnumerable<ProductInfoVM>> ProcessProductInfo(IEnumerable<ProductInfoVM> products)
        {
         
            // 應用標籤
            var tagDic = await products.GetTagDictionaryByProductsAsync(_tagConnectRepository);
            products = products.ApplyTags(tagDic);

            // 應用評論圖片
            var commentsByProduct = await products.GetCommentsCountByProductAsync(_productCommentRepository);
            products = products.ApplyCommentImages(commentsByProduct);

            return products;
        }


        private async Task<(List<SearchTagVM> tags, List<SearchCategoryVM> categories)> GetSelectedTagsAndCategoriesAsync(IEnumerable<ProductInfoVM> products)
        {
            var selectedTags = await products.GetTagsByProductsAsync(_tagRepository);
            var selectedCategories = await products.GetCategoriesByProductsAsync(_categoryRepository);
            return (selectedTags, selectedCategories);
        }

        private async Task<IEnumerable<ProductInfoVM>> GetProductDiscount(IEnumerable<ProductInfoVM> products)
        {
            var discounts = await products.GetDiscountsByProductsAsync(_productsDiscountRepository);
            products = products.ApplyDiscounts(discounts);
            return products;
        }


        public async Task<List<ProductInfoVM>> GetProductDataByPage(int page, int pageSize)
        {
            var cacheKey = $"{cacheKeyRedisStr()}-productByPage-{page}-{pageSize}";

            var cachedProducViewModel = await CacheHelper.TryGetFromCacheAsync<List<ProductInfoVM>>(_distributedCache, cacheKey);

            if (cachedProducViewModel != null)
            {
                return cachedProducViewModel;
            }

            var  products = (await _productRepository.ListAsync(x => x.ProductStatus == 1, page, pageSize)).CreateProductInfo();
            products = await GetProductDiscount(products);
            products = await ProcessProductInfo(products);

            await CacheHelper.SetCachedAsync(_distributedCache, cacheKey, products, slidingExpiration, absoluteExpirationRelativeToNow);
            return products.ToList();
        }


        public async Task<ProductSearchVM> GetProductData(int page, int pageSize, decimal min, decimal max)
        {

            var cacheKey = $"{cacheKeyRedisStr()}-product-{page}-{pageSize}-{min}-{max}";

            var cachedProducViewModel = await CacheHelper.TryGetFromCacheAsync<ProductSearchVM>(_distributedCache, cacheKey);

            if (cachedProducViewModel != null)
            {
                return cachedProducViewModel;
            }

            var isHasValue = min > 0 || max < 10000;

            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            int filterCount = 0;
            var totalCount = _productRepository.Query(p => p.ProductStatus == 1).Count();
            var products = new List<ProductInfoVM>().AsEnumerable();

            if (isHasValue)
            {

                products = (await _productRepository.ListAsync(p => p.ProductStatus == 1 && p.ProductPrice >= min && p.ProductPrice <= max)).CreateProductInfo();
              
                products = await GetProductDiscount(products);
            
                products = products.Where(p => p.SalsePrice >= min && p.SalsePrice <= max);

                filterCount = totalCount - products.Count();

            }
            else
            {
                products = (await _productRepository.ListAsync(x => x.ProductStatus == 1, page, pageSize)).CreateProductInfo();
                products = await GetProductDiscount(products);

            }

     
            products = await ProcessProductInfo(products);


            var model = CreateProductSearchViewModel(products.ToList(), null, null, totalCount);

            if (filterCount != 0) { model.FilterCount = filterCount; }

            await CacheHelper.SetCachedAsync(_distributedCache, cacheKey, model, slidingExpiration, absoluteExpirationRelativeToNow);

            return model;
        }

      


        public async Task<ProductSearchVM> GetProductDataByQuery(string gameKeywords)
        {
            // 生成 Redis 快取的唯一鍵，使用 gameKeywords 來標識不同查詢
            var cacheKey = $"{cacheKeyRedisStr()}-gameKeywords-{gameKeywords}";

            // 從 Redis 快取中檢查是否存在匹配的產品查詢結果
            var cachedProducViewModel = await CacheHelper.TryGetFromCacheAsync<ProductSearchVM>(_distributedCache, cacheKey);
            if (cachedProducViewModel != null)
            {
                // 如果在快取中找到結果，直接返回，不需要重新查詢數據庫
                return cachedProducViewModel;
            }

            // 將關鍵字轉換為小寫並去除空格，分割為多個有效的查詢關鍵字
            var keywords = gameKeywords.ToLower().Trim().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

         
            //這裡依照關鍵字做搜尋
            var products = (await _productRepository.ListAsync(p => keywords
            .Any(key => p.ProductName.ToLower().Contains(key)) && 
            p.ProductStatus == 1))
            .CreateProductInfo();

            // 查詢匹配產品的折扣信息
            products = await GetProductDiscount(products);

            products = await ProcessProductInfo(products);

            //查詢匹配產品的分類與標籤。
            var (selectedTags, selectedCategories) = await GetSelectedTagsAndCategoriesAsync(products);


            var model = CreateProductSearchViewModel(products.ToList(), selectedTags, selectedCategories);

            // 將查詢結果保存到 Redis 快取中
            await CacheHelper.SetCachedAsync(_distributedCache, cacheKey, model);

            return model;
        }





        public async Task<ProductSearchVM> GetProductDataByCategoryAndTag(List<int> categoryIds, List<int> tagIds, List<string> categoryNames, List<string> tagNames)
        {



            var selectedCategories = (await
                _categoryRepository.ListAsync(ca => categoryIds.Contains(ca.CategoryId) ||
                                                  categoryNames.Select(name => name.ToLower()).Contains(ca.CategoryName.ToLower()))).Distinct()
                                    .Select(c => new SearchCategoryVM { CategoryId = c.CategoryId, CategoryName = c.CategoryName })
                                    .ToList();



            var selectedTags = (await _tagRepository
                                .ListAsync(ta => tagIds.Contains(ta.TagId) ||
                                              tagNames.Select(name => name.ToLower()).Contains(ta.TagName.ToLower())))
                                .Distinct().Select(t => new SearchTagVM { TagId = t.TagId, TagName = t.TagName }).ToList();

            var selectedProductIdByTag = (await _tagConnectRepository.ListAsync(ta => selectedTags.Select(t => t.TagId).Contains(ta.TagId))).Select(ta => new TagConnect
            {
                ProductId = ta.ProductId,

                TagId = ta.TagId
            });

            var products = (await _productRepository.ListAsync(p => (selectedCategories.Select(c => c.CategoryId).Contains(p.CategoryId) || selectedProductIdByTag.Select(p => p.ProductId).Contains(p.ProductId)) && p.ProductStatus == 1)).Distinct().CreateProductInfo();

            products = await GetProductDiscount(products);


            products = await ProcessProductInfo(products);

            var model = CreateProductSearchViewModel(products.ToList(), selectedTags, selectedCategories);

            return model;
        }

        public async Task<ProductSearchVM> GetProductDataByFree(string type)
        {

            var cacheKey = $"{cacheKeyRedisStr()}-type-{type}";
            var cachedProducViewModel = await CacheHelper.TryGetFromCacheAsync<ProductSearchVM>(_distributedCache, cacheKey);
            if (cachedProducViewModel != null)
            {
                return cachedProducViewModel;
            }


            var products = (await _productRepository.ListAsync(p => p.ProductPrice <= 0 && p.ProductStatus == 1)).CreateProductInfo();

            products = await GetProductDiscount(products);


            products.ToList().RemoveAll(p => p.SalsePrice <= 0);


            products = await ProcessProductInfo(products);

            var (selectedTags, selectedCategories) = await GetSelectedTagsAndCategoriesAsync(products);

            var model = CreateProductSearchViewModel(products.ToList(), selectedTags, selectedCategories);

            await CacheHelper.SetCachedAsync(_distributedCache, cacheKey, model);

            return model;
        }





        public async Task<ProductSearchVM> GetProductDataBySellingHighest(string type)
        {
            var cacheKey = $"{cacheKeyRedisStr()}-type-{type}";
            var cachedProducViewModel = await CacheHelper.TryGetFromCacheAsync<ProductSearchVM>(_distributedCache, cacheKey);
            if (cachedProducViewModel != null)
            {
                return cachedProducViewModel;
            }

            var productIds = (await _productCollectionRepository.ListAsync()).GroupBy(p => p.ProductId).Select(
                g => new
                {
                    productId = g.Key,
                    SaleCount = g.Count()
                }).OrderByDescending(g => g.SaleCount)
                .Take(10).Select(p => p.productId);


            var now = DateOnly.FromDateTime(DateTime.Now);
            var oneYearAgo = now.AddYears(-1);


            var products = (await _productRepository.ListAsync(p => productIds.Contains(p.ProductId) && p.ProductStatus == 1 && p.ProductShelfTime > oneYearAgo &&
    p.ProductShelfTime < now)).CreateProductInfo();


            products = await GetProductDiscount(products);


            products = await ProcessProductInfo(products);

            var (selectedTags, selectedCategories) = await GetSelectedTagsAndCategoriesAsync(products);


            var model = CreateProductSearchViewModel(products.ToList(), selectedTags, selectedCategories);

            await CacheHelper.SetCachedAsync(_distributedCache, cacheKey, model);

            return model;
        }



        public async Task<ProductSearchVM> GetProductDataByNew(string type)
        {

            var cacheKey = $"{cacheKeyRedisStr()}-type-{type}";

            var cachedProducViewModel = await CacheHelper.TryGetFromCacheAsync<ProductSearchVM>(_distributedCache, cacheKey);
            if (cachedProducViewModel != null)
            {
                return cachedProducViewModel;
            }

            var now = DateTime.Now;
            var oneMonthAgo = now.AddMonths(-1); // 計算一個月前的時間
            var taggetTime = new DateOnly(oneMonthAgo.Year, oneMonthAgo.Month, oneMonthAgo.Day).AddDays(-1); ;
            var currTime = new DateOnly(now.Year, now.Month, now.Day).AddDays(1); ;

            var products = (await _productRepository.ListAsync(p => p.ProductShelfTime > taggetTime && p.ProductShelfTime < currTime && p.ProductStatus == 1)).OrderBy(p => p.ProductShelfTime).Take(10).CreateProductInfo();

            products = await GetProductDiscount(products);


            products = await ProcessProductInfo(products);

            var (selectedTags, selectedCategories) = await GetSelectedTagsAndCategoriesAsync(products);

            var model = CreateProductSearchViewModel(products.ToList(), selectedTags, selectedCategories);

            await CacheHelper.SetCachedAsync(_distributedCache, cacheKey, model);

            return model;
        }



        public async Task<ProductSearchVM> GetProductDataBySoon(string type)
        {
            var cacheKey = $"{cacheKeyRedisStr()}-type-{type}";
            var cachedProducViewModel = await CacheHelper.TryGetFromCacheAsync<ProductSearchVM>(_distributedCache, cacheKey);
            if (cachedProducViewModel != null)
            {
                return cachedProducViewModel;
            }
            var products = (await _productRepository.ListAsync(p => p.ProductShelfTime > DateOnly.FromDateTime(DateTime.Now).AddDays(1) && p.ProductStatus == 1)).OrderBy(p => p.ProductShelfTime).CreateProductInfo();

            products = await GetProductDiscount(products);

            products = await ProcessProductInfo(products);
           
            var (selectedTags, selectedCategories) = await GetSelectedTagsAndCategoriesAsync(products);


            var model = CreateProductSearchViewModel(products.ToList(), selectedTags, selectedCategories);

            await CacheHelper.SetCachedAsync(_distributedCache, cacheKey, model);

            return model;
        }


        public async Task<ProductSearchVM> GetProductDataByDiscount(string type)
        {
            var cacheKey = $"{cacheKeyRedisStr()}-type-{type}";
            var cachedProducViewModel = await CacheHelper.TryGetFromCacheAsync<ProductSearchVM>(_distributedCache, cacheKey);
            if (cachedProducViewModel != null)
            {
                return cachedProducViewModel;
            }

            var currTime = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
            var discounts = (await _productsDiscountRepository.ListAsync
                (p => p.SalesStartDate < currTime &&
                p.SalesEndDate > currTime &&
                p.Discount < 1))
                .Select(d => new ProductsDiscount { ProductId = d.ProductId, Discount = d.Discount });

            var products = (await _productRepository.ListAsync(p => discounts.Select(d => d.ProductId).Contains(p.ProductId) && p.ProductStatus == 1)).Distinct().CreateProductInfo();

            products = products.ApplyDiscounts(discounts);

        
            products = await ProcessProductInfo(products);

            var (selectedTags, selectedCategories) = await GetSelectedTagsAndCategoriesAsync(products);


            var model = CreateProductSearchViewModel(products.ToList(), selectedTags, selectedCategories);

            await CacheHelper.SetCachedAsync(_distributedCache, cacheKey, model);
            return model;
        }


        public async Task<List<ProductInfoVM>> GetProductBySuggestions(string keywords)
        {

            var cacheKey = $"{cacheKeyRedisStr()}-Keywords-{keywords}";

            var cachedProducViewModel = await CacheHelper.TryGetFromCacheAsync<List<ProductInfoVM>>(_distributedCache, cacheKey);

       

            if (cachedProducViewModel != null)
            {
                return cachedProducViewModel;
            }

            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            var keyword = keywords.ToLower().Trim().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var products = (await _productRepository.ListAsync(p => keyword.Any(key => p.ProductName.ToLower().Contains(key)) && p.ProductStatus == 1)).CreateProductInfo();

            var discounts = await products.GetDiscountsByProductsAsync(_productsDiscountRepository);

            var model = products.ApplyDiscounts(discounts).ToList();

            await CacheHelper.SetCachedAsync(_distributedCache, cacheKey, model);
      
            return model;
        }

    }
}






