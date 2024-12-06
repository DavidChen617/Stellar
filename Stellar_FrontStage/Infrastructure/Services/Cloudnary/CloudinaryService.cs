using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Infrastructure.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public string UploadImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return null;
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(image.FileName,image.OpenReadStream())
            };

            var uploadResult = _cloudinary.Upload(uploadParams);
            return uploadResult.SecureUrl.AbsoluteUri;  // 返回圖片的 URL
        }
        // 上傳音頻
        public string UploadAudio(IFormFile audio)
        {
            if (audio == null || audio.Length == 0)
                return null;

            var uploadParams = new RawUploadParams()
            {
                File = new FileDescription(audio.FileName, audio.OpenReadStream())
            };

            var uploadResult = _cloudinary.Upload(uploadParams);
            return uploadResult.SecureUrl.AbsoluteUri;  // 返回音頻的 URL
        }


        //public async Task<string> UploadAudioAsync(Stream audioStream, string fileName)
        //{
        //    if (audioStream == null || audioStream.Length == 0)
        //    {
             
        //        return null;
        //    }

        //    var uploadParams = new RawUploadParams
        //    {
        //        File = new FileDescription(fileName, audioStream),
        //        ResourceType = ResourceType.Auto // 自動檢測資源類型
        //    };

        //    try
        //    {
        //        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        //        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
        //        {
                  
        //            return uploadResult.SecureUrl.AbsoluteUri;
        //        }
        //        else
        //        {
                 
        //            return null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
             
        //        return null;
        //    }
        //}
    }
}
