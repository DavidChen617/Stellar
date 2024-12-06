using System;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Web.Services.Translation
{
    public class TranslationService
    {
        private readonly string _huggingFaceApiKey;
        private readonly HttpClient _httpClient;
        public TranslationService(IConfiguration configuration)
        {
        
            _huggingFaceApiKey = configuration["HuggingFace:ApiKey"];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _huggingFaceApiKey);
        }

        public async Task<string> TranslateToChineseAsync(string text)
        {
            var requestBody = new
            {
                inputs = text
            };

            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("https://api-inference.huggingface.co/models/Helsinki-NLP/opus-mt-en-zh", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var json = JArray.Parse(jsonResponse);
                    var translatedText = json[0]["translation_text"].ToString();

                  
                    return translatedText;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                
                    throw new Exception($"Hugging Face API 錯誤: {error}");
                }
            }
            catch (Exception ex)
            {
          
                throw new Exception("翻譯失敗，請稍後再試。");
            }
        }

    
    }
}
