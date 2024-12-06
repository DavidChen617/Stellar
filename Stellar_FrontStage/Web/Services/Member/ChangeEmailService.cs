using ApplicationCore.Dtos.Mail;
using ApplicationCore.Interfaces;
using Infrastructure.Data.Mail;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Mail;
using System.Web;
using Web.Helpers;
using Web.ViewModels.Member;
using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Web.Services.Member
{
    public class ChangeEmailService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<VerifyMail> _verifyMailRepository;
        private readonly IEmailSenderService _emailSenderService;
        private VerifyEmailSettings _verifyEmailSettings;

        public ChangeEmailService(IRepository<User> userRepository, IRepository<VerifyMail> verifyMailRepository, IEmailSenderService emailSenderService, VerifyEmailSettings verifyEmailSettings)
        {
            _userRepository = userRepository;
            _verifyMailRepository = verifyMailRepository;
            _emailSenderService = emailSenderService;
            _verifyEmailSettings = verifyEmailSettings;
        }


       

        public async Task<ChangeEmailViewModel> GetChangeEmailData(int Id)
        {
            int IdOfLoginUser = Id;

            var user = await _userRepository.FirstOrDefaultAsync(x => x.UserId == IdOfLoginUser);

            var GetChangeEmailData = new ChangeEmailViewModel
            {
                UserNickName = user.NickName,
                UserID = user.UserId,
                EmailAddress = user.EmailAddress,
            };

            return GetChangeEmailData;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        //驗證

        //public async Task<string> UpdateEmailAddress(int userID, string emailAddress)
        //{


        //    var user = await _userRepository.FirstOrDefaultAsync(x => x.UserId == userID);


        //    // 檢查是否有其他使用者使用相同的電子郵件地址
        //    var isEmailExist = (from u in await _userRepository.ListAsync()
        //                        where u.EmailAddress == emailAddress
        //                        select u).Any();

        //    if (isEmailExist)
        //    {
        //        // 電子郵件已存在，返回錯誤訊息
        //        return "The email address is already in use.";
        //    }
        //    else
        //    {

        //        return "Email address updated successfully.";
        //    }
        //}

        public async Task<string> SendCheckEmail(int userID, string emailAddress)
        {
            try
            {
                Console.WriteLine($"Starting SendCheckEmail for userID: {userID} with email: {emailAddress}");

                var user = await _userRepository.FirstOrDefaultAsync(x => x.UserId == userID);
                if (user == null)
                {
                    Console.WriteLine("User not found.");
                    return "User not found.";
                }

                // 檢查是否有其他使用者使用相同的電子郵件地址
                var isEmailExist = (from u in await _userRepository.ListAsync()
                                    where u.EmailAddress == emailAddress && u.UserId != userID
                                    select u).Any();
                if (isEmailExist)
                {
                    Console.WriteLine("The email address is already in use.");
                    return "The email address is already in use.";
                }

                var ExpireTime = DateTime.UtcNow.AddMinutes(10);

                var verifyEmailDto = new VerifyEmailDto()
                {
                    Email = emailAddress,
                    ExpireTime = ExpireTime
                };

                var serializeStr = Base64SerializerHelpers.SerializeInput(verifyEmailDto);
                var encodedParameter = HttpUtility.UrlEncode(serializeStr);
                // 使用 ReturnCheckEmailUrl 來生成驗證連結
                var uri = _verifyEmailSettings.ReturnCheckChangeEmailUrl;
                var returnUrl = QueryHelpers.AddQueryString(uri, "encodingParameter", serializeStr);
                Console.WriteLine($"Generated verification link: {returnUrl}");

                var emailTemplate = EmailTemplateHelper.ChangePasswordVerifyEmail(user.NickName, returnUrl);

                var email = new SendEmailRequest()
                {
                    Receiver = user.NickName,
                    Body = emailTemplate,
                    Subject = "Stellar帳戶信箱驗證來信",
                    ReceiverEmail = emailAddress,
                };

                // 在 VerifyMail 表中存儲新的電子郵件地址
                var verifyMailData = new VerifyMail()
                {
                    UserId = userID,
                    EncodingParameter = serializeStr,
                    Expired = ExpireTime,
                    NewEmailAddress = emailAddress // 將新電子郵件地址存入 VerifyMail 表
                };

                Console.WriteLine("Adding verify mail data to repository.");
                await _verifyMailRepository.AddAsync(verifyMailData);

                Console.WriteLine("Sending verification email.");
                await _emailSenderService.SendEmailAsync(email);

                Console.WriteLine("Verification email sent successfully.");
                return "Verification email sent successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send verification email: {ex.Message} - StackTrace: {ex.StackTrace}");
                return "An error occurred while sending the verification email.";
            }
        }


        public async Task<bool> VerifyCheckEmail(string encodingParameter)
        {
            try
            {
                // 解碼參數並反序列化
                var verifyEmailDto = DeSerializeURLEncodeParameter<VerifyEmailDto>(encodingParameter);
                if (verifyEmailDto == null)
                {
                    Console.WriteLine("Decoded email DTO is null.");
                    return false;
                }

                // 從資料庫查詢驗證記錄
                var verifyDataInDB = await _verifyMailRepository.FirstOrDefaultAsync(x => x.EncodingParameter == encodingParameter);
                if (verifyDataInDB == null)
                {
                    Console.WriteLine("No verification data found in database.");
                    return false;
                }

                // 獲取用戶資料
                var user = await _userRepository.FirstOrDefaultAsync(x => x.UserId == verifyDataInDB.UserId);
                if (user == null)
                {
                    Console.WriteLine("User not found in database.");
                    return false;
                }

                // 驗證參數是否匹配
                var verifyEmailDtoInDB = DeSerializeURLEncodeParameter<VerifyEmailDto>(verifyDataInDB.EncodingParameter);
                if (verifyEmailDtoInDB.Email != verifyEmailDto.Email)
                {
                    Console.WriteLine("Email addresses do not match.");
                    return false;
                }

                // 檢查驗證連結是否過期
                if (verifyEmailDto.ExpireTime < DateTime.UtcNow)
                {
                    Console.WriteLine("Verification link has expired.");
                    return false;
                }

                // 如果驗證通過，從 VerifyMail 表中獲取新的電子郵件地址並更新
                Console.WriteLine($"Updating user {user.UserId} email from {user.EmailAddress} to {verifyDataInDB.NewEmailAddress}");
               
                //var verifyEmailData = (from v in _verifyMailRepository.List(x => x.UserId == user.UserId)
                //                       orderby v.VerifyId descending
                //                       select v).FirstOrDefault();



                user.EmailAddress = verifyDataInDB.NewEmailAddress;               
                user.State = 1; // 假設 1 表示已認證

                // 更新資料庫
                await _userRepository.UpdateAsync(user);

                // 增加確認更新是否完成的日誌
                var updatedUser = await _userRepository.FirstOrDefaultAsync(x => x.UserId == user.UserId);
                if (updatedUser != null)
                {
                    Console.WriteLine($"Updated Email in DB: {updatedUser.EmailAddress}, State: {updatedUser.State}");
                }
                else
                {
                    Console.WriteLine("Failed to find the updated user record.");
                }

                return true;
            }
            catch (Exception ex)
            {
                // 記錄錯誤信息並返回 false
                Console.WriteLine($"Error during email verification: {ex.Message}");
                return false;
            }
        }






        private T DeSerializeURLEncodeParameter<T>(string encodingParameter) where T : class
        {
            var decodeStr = HttpUtility.UrlDecode(encodingParameter);
            return Base64SerializerHelpers.DeSerializeParameter<T>(decodeStr);
        }







    }
}
