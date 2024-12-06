using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Drawing.Printing;
using Web.Extensions;
using Web.Services.Member;
using Web.Services.Search;
using Web.ViewModels.Product;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Web.ControllersApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductSearchAPIController : ControllerBase
    {
        private readonly ProductSearchServices _productSearchService;
        private readonly ILogger<ProductSearchAPIController> _logger;
        public ProductSearchAPIController(ProductSearchServices productSearchService, ILogger<ProductSearchAPIController> logger)
        {
            _productSearchService = productSearchService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<ActionResult<T>> ExecuteWithLogging<T>(Func<Task<T>> func)
        {
            try
            {
                // 執行傳遞進來的函數，並等待其返回結果
                var result = await func();
                // 如果有結果則返回 200 (OK)，如果沒有則返回 404 (NotFound)
                return result != null ? Ok(result) : NotFound();
            }
            catch (Exception ex)
            {
                // 捕獲異常並記錄錯誤信息
                _logger.LogError(ex, "An error occurred while processing the request.");
                // 返回 500 (Internal Server Error) 狀態碼
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpGet("getProduct/page/{page}/pageSize/{pageSize}/min/{min}/max/{max}")]
        public async Task<ActionResult<ProductSearchVM>> GetProductData(decimal min, decimal max, int page = 1, int pageSize = 10)
        {
            return await ExecuteWithLogging(async () =>
            {
                var model = await _productSearchService.GetProductData(page, pageSize, min, max);
                return model;
            });
        }

        [HttpGet("getProductDataByPage/page/{page}/pageSize/{pageSize}")]
        public async Task<ActionResult<List<ProductInfoVM>>> GetProductDataByPage(int page , int pageSize = 10)
        {
            return await ExecuteWithLogging(async () =>
            {
                var model = await _productSearchService.GetProductDataByPage(page, pageSize);
                return model;
            });
        }





        [HttpGet("query/{keyword}/min/{min}/max/{max}")]
        public async Task<ActionResult<ProductSearchVM>> GetProductDataByQuery(string keyword = "", decimal min = 0, decimal max = 0)
        {
            // 判斷是否有價格範圍的限制
            var isHasValue = min > 0 || max > 0;
            var model = await _productSearchService.GetProductDataByQuery(keyword);
            if (isHasValue)
            {
                // 如果有價格範圍，則對查詢結果進行篩選
                model.GetProductByRange(min, max);
            }

            // 使用通用的 ExecuteWithLogging 方法進行日誌記錄和錯誤處理
            return await ExecuteWithLogging(async () =>
            {
                return model;
            });
        }


        [HttpGet("CategoryOrTag/categoryIds/{categoryIds}/tagIds/{tagIds}/categoryNames/{categoryNames}/tagNames/{tagNames}/min/{min}/max/{max}")]
        public async Task<ActionResult<ProductSearchVM>> GetProductDataByCategoryAndTag(string categoryIds, string tagIds, string categoryNames, string tagNames, decimal min = 0, decimal max = 0)
        {

            var isHasValue = min > 0 || max > 0;
            List<int> categoryIdsArray = ConvertToIntList(categoryIds);
            List<int> tagIdsArray = ConvertToIntList(tagIds);
            List<string> categoryNamesArray = ConvertToStringList(categoryNames);
            List<string> tagNamesArray = ConvertToStringList(tagNames);
            var model = await _productSearchService.GetProductDataByCategoryAndTag(categoryIdsArray, tagIdsArray, categoryNamesArray, tagNamesArray);
            model.GetProductByRange(min, max);
        
            return await ExecuteWithLogging(async () =>
            {
                return model;
            });
        }



        [HttpGet("typeBy/{type}/min/{min}/max/{max}")]
        public async Task<ActionResult<ProductSearchVM>> GetProductDataByType(string type, decimal min = 0, decimal max = 0)
        {
            return await ExecuteWithLogging(async () =>
            {
                ProductSearchVM model;
                var isHasValue = min > 0 || max > 0;
                switch (type)
                {
                    case "free":
                        model = await _productSearchService.GetProductDataByFree(type);
                        break;
                    case "highest":
                        model = await _productSearchService.GetProductDataBySellingHighest(type);
                        break;
                    case "New":
                        model = await _productSearchService.GetProductDataByNew(type);
                        break;
                    case "Soon":
                        model = await _productSearchService.GetProductDataBySoon(type);
                        break;
                    case "Discount":
                        model = await _productSearchService.GetProductDataByDiscount(type);
                        break;
                    default:
                        model = await _productSearchService.GetProductData(1, 10, min, max); // 可選的預設處理
                        break;
                }

                if (isHasValue)
                {
                    model.GetProductByRange(min, max);
                }

                return model;
            });
        }


        [HttpGet("Suggestions/{keywords}")]
        public async Task<ActionResult<List<ProductInfoVM>>> GetProductBySuggestions(string keywords)
        {
            return await _productSearchService.GetProductBySuggestions(keywords);

        }




     

        private List<int> ConvertToIntList(string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? new List<int>()
                : input
                    .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();
        }

        private List<string> ConvertToStringList(string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? new List<string>()
                : input
                    .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }




    }
}

