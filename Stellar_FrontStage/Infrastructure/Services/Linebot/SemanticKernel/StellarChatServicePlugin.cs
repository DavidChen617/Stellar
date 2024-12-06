using Infrastructure.Service.MongoDB;
using Infrastructure.Services.Linebot.SemanticProductSearch;
using Infrastructure.Services.Linebot.SemanticProductSearch.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using HandlebarsDotNet.Helpers.Helpers;
namespace Infrastructure.Services.Linebot.SemanticKernel
{
    [Experimental("SKEXP0020")]
    public class StellarChatServicePlugin
    {
        private readonly SemanticProductSearchService _semanticProductSearchService;
        private readonly SemanticKernelSearchService _mongoDBService;
        private readonly Microsoft.Extensions.Hosting.IHostEnvironment _env;
        public StellarChatServicePlugin(SemanticProductSearchService semanticProductSearchService, SemanticKernelSearchService mongoDBService, IHostEnvironment env)
        {
            _semanticProductSearchService = semanticProductSearchService;
            _mongoDBService = mongoDBService;
            _env = env;
        }


        [KernelFunction("GetProductRecommendationsByUserInput")]
        [Description("Get product(game) recommendations by user input")]
        public async Task<List<ProductSearchResult>> GetProductRecommendationsByUserInput(
            [Description("The user input")]string userInput)
        {
            
            return await _semanticProductSearchService.GetRecommendationsAsync(userInput);
        }

        [KernelFunction("Todays_datetime")]
        [Description("Retrieves the current time in UTC.")]
        public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");

        [KernelFunction("CurrentUser_Orders")]
        [Description("getCurrentUserOrder")]
        public async Task<string> GetOrderData([Description("The User ID from input")]int askUserId, [Description("The LoginUserId")] int LoginuserId)
        {
            if(LoginuserId== askUserId) { 
           var Order= await _mongoDBService.GetOrderResult(LoginuserId);
                return Order;
            }
            else
            {
                return "NoAuthorize!";
            }
            
        }
        [KernelFunction("GetWebPagePath")]
        [Description("Get WebPage Path by user input")]
        public async Task<string> GetStellarPath([Description("Questions related to webpage paths and customer service for a gaming website.")]string input)
        {
            try {
                var contentRootPath = _env.ContentRootPath;
                var FilePath=Path.Combine(contentRootPath, "Helpers", "PluginData", "Stellar客服規範.txt");

                //var FilePath = DirPath +"/Helpers/PluginData/Stellar客服規範.txt";
                
            string content=await File.ReadAllTextAsync(FilePath);
                return content;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }


    }
}
