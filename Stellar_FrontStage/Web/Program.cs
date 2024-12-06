using CloudinaryDotNet;
using Coravel;
using Infrastructure;
using Infrastructure.Data.Cloudnary;
using Infrastructure.Data.Mail;
using Infrastructure.Data.MailKit;
using Microsoft.Extensions.Options;
using Web.Configurations;
using Web.Hubs;
using Web.MemoryCatch;
namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddInfrastructureService(builder.Configuration);
           
            //cloudnary
            builder.Services
            .Configure<CloudinarySettings>(builder.Configuration.GetSection(nameof(CloudinarySettings)))
    .       AddSingleton(settings => settings.GetRequiredService<IOptions<CloudinarySettings>>().Value);
            builder.Services.AddSingleton(sp =>
            {
                var cloudinarySettings = sp.GetRequiredService<CloudinarySettings>();
                return new Cloudinary(new Account(cloudinarySettings.CloudName, cloudinarySettings.ApiKey, cloudinarySettings.ApiSecret));
            });


            builder.Services.AddWebService(builder.Configuration);
            builder.Services.AddApplicationCoreService(builder.Configuration);
            //MailKit
            //Ioption指的就是appsetting的值帶入方式用class的欄位帶入 這樣可以不用一個一個寫出來
            builder.Services.Configure<MailServerSettings>(builder.Configuration.GetSection(MailServerSettings.MailServerSettingsKey));
            builder.Services.Configure<VerifyEmailSettings>(builder.Configuration.GetSection(VerifyEmailSettings.VerifyEmailSettingsKey))
                .AddSingleton(provider => provider.GetRequiredService<IOptions<VerifyEmailSettings>>().Value);

          
            builder.Services.AddRazorPages();

            builder.Services.AddHttpClient();
            builder.Services.AddAutoMapper(typeof(Program)); // 注入AutoMapper
            //Coravel
            builder.Services.AddScheduler();
            builder.Services.AddScoped<Invocable>();

            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<IHomeCacheService, HomeCacheService>();


            string[] urls = new[]
            {
                "http://127.0.0.1:5500",
                "http://127.0.0.1:5502",
                "https://stellarstellaradmin.azurewebsites.net",
                "https://agent.build-school.com/"
            };

            builder.Services.AddCors(options =>
                options.AddDefaultPolicy(builder =>
                    builder.WithOrigins(urls)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()));


            //聊天室用的SignalR
            builder.Services.AddSignalR();
            //builder.Services.AddScoped<HomeService, HomeService>();
            //builder.Services.AddScoped<StoreNavbarService, StoreNavbarService>();
            //builder.Services.AddScoped<LayoutService, LayoutService>();


            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //swagger
            //app.UseSwagger();
            //app.UseSwaggerUI();




            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors();



            app.UseRouting();


            //ErorPage
            //處理 ASP.NET Core 中的錯誤 - https://docs.microsoft.com/zh-tw/aspnet/core/fundamentals/error-handling?view=aspnetcore-6.0
            //處理 ASP.NET Core web api 中的錯誤 - https://docs.microsoft.com/zh-tw/aspnet/core/web-api/handle-errors?view=aspnetcore-6.0

            //app.UseStatusCodePagesWithRedirects("~/Errors/Error404/{0}");

            //UseStatusCodePagesWithReExecute方必須用/開頭, 不能有~符號
            app.UseStatusCodePagesWithReExecute("/Errors/ErrorPage", "?statuscode={0}");

            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.MapRazorPages(); // 註冊 Razor Pages 路由

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            //設定聊天室的hub
            app.MapHub<ChatHub>("/chatHub");

            //Coravel
            app.Services.UseScheduler(scheduler =>
            {
                scheduler.Schedule<Invocable>().Daily();
            });

            app.Run();
        }
    }
}
