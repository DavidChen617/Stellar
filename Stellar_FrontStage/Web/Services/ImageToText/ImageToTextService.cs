using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using Newtonsoft.Json.Linq;
using Web.Services.Translation;

public class ImageToTextService
{
    private readonly string _huggingFaceApiKey;
    private readonly HttpClient _httpClient;
    private readonly TranslationService _translationService;

    public ImageToTextService(TranslationService translationService, IConfiguration configuration)
    {
        _huggingFaceApiKey = configuration["HuggingFace:ApiKey"];
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration["HuggingFace:ApiKey"]);
        _translationService = translationService;
    }

    public async Task<string> ConvertImageToTextAsync(string imageUrl)
    {
        var requestBody = new
        {
            inputs = imageUrl
        };

        var response = await _httpClient.PostAsJsonAsync("https://api-inference.huggingface.co/models/Salesforce/blip-image-captioning-base", requestBody);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var json = JArray.Parse(jsonResponse);
            var englishDescription = json[0]["generated_text"].ToString();

            // 使用 TranslationService 翻譯英文描述為中文
            var chineseDescription = await _translationService.TranslateToChineseAsync(englishDescription);
            //將簡體轉成繁體
            string TraditionalText = ChineseConverter.Convert(chineseDescription, ChineseConversionDirection.SimplifiedToTraditional); // 轉成繁體
            return TraditionalText;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Hugging Face API 錯誤: {error}");
        }
    }
}
