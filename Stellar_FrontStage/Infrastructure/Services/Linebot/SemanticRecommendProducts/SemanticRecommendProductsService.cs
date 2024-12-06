using Infrastructure.Data.Mongo.Entity;
using Infrastructure.Data.Mongo.Repository;
using Infrastructure.Data.Mongo;
using Infrastructure.Service.MongoDB;
using Infrastructure.Services.Linebot.SemanticProductSearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Microsoft.SemanticKernel.Memory;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using Infrastructure.Services.Linebot.SemanticProductSearch.Dtos;
using Infrastructure.Services.Linebot.SemanticRecommendProducts.Dtos;
using Infrastructure.Services.DapperSemanticKernelRecommendProducts;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Infrastructure.Services.Linebot.SemanticRecommendProducts
{
    [Experimental("SKEXP0020")]
    public class SemanticRecommendProductsService
    {
        private readonly SemanticKernelRecommendProductsService _mongoDBService;
        private readonly MemoryBuilder _memoryBuilder;
        private readonly ISemanticTextMemory _semanticTextMemory;
        private readonly MongoDbVectorSettings _mongoDbSettings;
        private readonly MongoDBMemoryStore _mongoDBMemoryStore;
        private readonly string _openAiApiKey;
        private readonly string _connectionString;
        private readonly string _searchIndexName;
        private readonly string _databaseName;
        private readonly string _collectionName;
        private readonly IMongoClient _mongoClient;
        private readonly ILogger<SemanticRecommendProductsService> _logger;

        public SemanticRecommendProductsService(MongoDbVectorSettings mongoDbSettings, IMongoClient mongoClient,
            IConfiguration configuration, ILogger<SemanticRecommendProductsService> logger, MongoRepository<RecommendProducts> productInMongoRepository, SemanticKernelRecommendProductsService mongoDBService)
        {
            // Initialize the openAI API key for text embedding generation
            _openAiApiKey = configuration["OpenAIApiKey"];
            // Initialize the mongodb settings: connection string, search index name, database name, collection name
            _mongoDbSettings = mongoDbSettings;
            _connectionString = _mongoDbSettings.ConnectionString;
            _searchIndexName = _mongoDbSettings.SearchIndexName;
            _databaseName = _mongoDbSettings.DatabaseName;
            //_collectionName = _mongoDbSettings.VectorCollectionName;
            // Initialize the memory store: MongoDBMemoryStore(or you can use other memory store like QdrantMemoryStore, etc.)
            _mongoDBMemoryStore = new MongoDBMemoryStore(_connectionString, _databaseName, _searchIndexName);
            // Initialize the memory builder: set up text embedding generation and memory store
            _memoryBuilder = new MemoryBuilder();
            _memoryBuilder.WithOpenAITextEmbeddingGeneration("text-embedding-ada-002", _openAiApiKey);
            _memoryBuilder.WithMemoryStore(_mongoDBMemoryStore);
            // Build the memory: create the semantic text memory
            _semanticTextMemory = _memoryBuilder.Build();
            _mongoClient = mongoClient;
            _logger = logger;
            _mongoDBService = mongoDBService;
        }

        public async Task<List<int>> GetRecommendationsAsync(int id,string description)
        {
            try
            {
                var memories = _semanticTextMemory.SearchAsync("RecommendProducts", description, limit: 5, minRelevanceScore: 0.8);


                var result = new List<int>();

                await foreach (var memory in memories)
                {
                    int memoryId = Int32.Parse(memory.Metadata.Id);

                    if (memoryId != id)
                    {
                        result.Add(memoryId);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during memory search: {ex.Message}");
            }
            return null;
        }
        public async Task FetchAndSaveProductDocumentsAsync()
        {
            // Fetch and save product documents to the semantic text memory
            await FetchAndSaveProductDocuments(_semanticTextMemory);
        }

        private async Task FetchAndSaveProductDocuments(ISemanticTextMemory memory)
        {
            IEnumerable<RecommendProducts> products = await _mongoDBService.GetFetchData();
            foreach (var product in products)
            {
                try
                {
                    _logger.LogInformation($"Processing {product.ProductId}, {product.ProductName}...");
                    await memory.SaveInformationAsync(
                        collection: "RecommendProducts",
                        text: $"""
                        產品ID：{product.ProductId},
                        產品原始價格：{product.ProductPrice},
                        產品折扣後價格：{product.SalesPrice},
                        圖片：{product.ProductMainImageUrl},
                        描述：{product.Description},
                        產品名稱：{product.ProductName},
                        折扣：{product.Discount}
                        """,
                        id: product.ProductId.ToString(),
                        description: product.Description
                    );
                    _logger.LogInformation($"Done {product.ProductId}{product.SalesPrice}...");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }
    }
}
