﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Infrastructure.Services;

namespace Web.ControllersApi
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpeechToTextController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public SpeechToTextController(IHttpClientFactory httpClientFactory, IConfiguration configuration, CloudinaryService cloudinaryService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _cloudinaryService = cloudinaryService;
        }


        [HttpPost("upload-audio")]
        public async Task<IActionResult> UploadAudio([FromForm] AudioUploadModel model)
        {
            if (model.Audio == null || model.Audio.Length == 0)
            {
                return BadRequest("未收到音频文件。");
            }

            try
            {       
                var cloudinaryUrl =  _cloudinaryService.UploadAudio(model.Audio);


                if (string.IsNullOrEmpty(cloudinaryUrl))
                {
                    return StatusCode(500, "上傳音頻文件到 Cloudinary 時出錯。");
                }
            
                // 調用 OpenAI Whisper API 进行轉錄
                var transcription = await TranscribeAudioAsync(cloudinaryUrl);

                return Ok(new { transcript = transcription });
            }
            catch (Exception ex)
            {
              
              
                return StatusCode(500, "處理音頻文件時出錯。");
            }
        }



        private async Task<string> TranscribeAudioAsync(string audioUrl)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("OpenAI API 金鑰未配置。");
            }

            var apiUrl = "https://api.openai.com/v1/audio/transcriptions";

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            byte[] audioData;
            try
            {
               
                audioData = await httpClient.GetByteArrayAsync(audioUrl);
            
            }
            catch (Exception ex)
            {
                throw new Exception($"從 Cloudinary 下載音頻文件時出錯: {ex.Message}");
            }


            using var form = new MultipartFormDataContent();
            var audioContent = new ByteArrayContent(audioData);
            audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
            form.Add(audioContent, "file", "downloaded_audio.wav");
            form.Add(new StringContent("whisper-1"), "model");
            form.Add(new StringContent("zh"), "language");

            var response = await httpClient.PostAsync(apiUrl, form);

            if (response.IsSuccessStatusCode)
            {
                var transcriptionResponse = await response.Content.ReadFromJsonAsync<TranscriptionResponse>();
                return transcriptionResponse?.Text ?? "沒有轉錄結果。";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenAI API 錯誤: {response.StatusCode}\n{errorContent}");
            }
        }


       
        public class AudioUploadModel
        {
            public IFormFile Audio { get; set; }
        }

     
        public class TranscriptionResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("text")]
            public string Text { get; set; }
        
        }
    }
}


//[HttpPost("upload-audio")]

//public async Task<IActionResult> UploadAudio([FromForm] AudioUploadModel model)
//{
//    if (model.Audio == null || model.Audio.Length == 0)
//    {
//        return BadRequest("未收到音頻文件。");
//    }

//    // 保存臨時音頻文件
//    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
//    if (!Directory.Exists(uploadsFolder))
//    {
//        Directory.CreateDirectory(uploadsFolder);
//    }

//    var uniqueFileName = $"{Guid.NewGuid()}_{model.Audio.FileName}";
//    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
//    using (var stream = new FileStream(filePath, FileMode.Create))
//    {
//        await model.Audio.CopyToAsync(stream);
//    }
//    try
//    {
//        // 使用 CloudinaryService 上傳音頻文件
//        var cloudinaryUrl = _cloudinaryService.UploadAudio(model.Audio);
//        if (string.IsNullOrEmpty(cloudinaryUrl))
//        {
//            return StatusCode(500, "上傳音頻文件到 Cloudinary 時出錯。");
//        }
//        Console.WriteLine($"上傳成功，Cloudinary URL: {cloudinaryUrl}");

//        // 調用 OpenAI Whisper API 進行轉錄
//        var transcription = await TranscribeAudioAsync(cloudinaryUrl);
//        return Ok(new { transcript = transcription });
//    }
//    catch (Exception ex)
//    {
//        return StatusCode(500, $"處理音頻文件時出錯: {ex.Message}");
//    }
//    finally
//    {
//        // 刪除臨時音頻文件
//        if (System.IO.File.Exists(filePath))
//        {
//            System.IO.File.Delete(filePath);
//        }
//    }

//}