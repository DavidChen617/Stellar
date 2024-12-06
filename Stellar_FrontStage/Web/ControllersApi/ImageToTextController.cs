using Infrastructure.Services;

using Microsoft.AspNetCore.Mvc;


namespace Web.ControllersApi
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageToTextController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;
        private readonly ImageToTextService _imageToTextService;

        public ImageToTextController(CloudinaryService cloudinaryService, ImageToTextService imageToTextService)
        {
            _cloudinaryService = cloudinaryService;
            _imageToTextService = imageToTextService; 
        }

        [HttpPost("convert")]
        public async Task<IActionResult> ConvertImageToText([FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("請上傳有效的圖片");
            }

            try
            {
                // 1. 上傳圖片到 Cloudinary，取得圖片的 URL
                string imageUrl = _cloudinaryService.UploadImage(imageFile);
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return StatusCode(500, "圖片上傳失敗");
                }

                // 2. 使用圖片 URL 呼叫圖片轉文字服務
                string textResult = await _imageToTextService.ConvertImageToTextAsync(imageUrl);
                return Ok(new { imageUrl, description = textResult });
             
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"圖片轉文字失敗: {ex.Message}");
                return StatusCode(500, "圖片轉文字失敗，請稍後再試。");
            }
        }
    }

}
