using Coravel.Invocable;
using Infrastructure.Service.MongoDB;
using Infrastructure.Services.Linebot.SemanticProductSearch;
using Infrastructure.Services.Linebot.SemanticRecommendProducts;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;

namespace Web
{   
    [Experimental("SKEXP0001")]
    public class Invocable : IInvocable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SemanticProductSearchService _semanticProductSearchService;
        private readonly SemanticRecommendProductsService _semanticRecommendProductsService;
        public Invocable(IServiceProvider serviceProvider, SemanticProductSearchService semanticProductSearchService, SemanticRecommendProductsService semanticRecommendProductsService)
        {
            _serviceProvider = serviceProvider;
            _semanticProductSearchService = semanticProductSearchService;
            _semanticRecommendProductsService = semanticRecommendProductsService;
        }

        public async Task Invoke()
        {
            try
            {

                //將產品資訊丟給mongo
                    await _semanticProductSearchService.UpdateProductAsync();
                    await _semanticRecommendProductsService.FetchAndSaveProductDocumentsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Task encountered an error: {ex.Message}");
            }
        }
    }
}
