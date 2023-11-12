using ChatAppTest;
using ChatAppTest.Controllers;
using ChatAppTest.FunctionController.Chat;
using ChatAppTest.FunctionController.Session;
using ChatAppTest.FunctionController.User;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

//いろいろ初期化　タイマーとか起動
ChatController.Initialize();
ChatSessionController.Initialize();
ChatUserController.Initialize();
ChatReadCountController.Initialize();

//スレッドプール設定
ThreadPool.GetMinThreads(out var _, out var completionPortThreads);
ThreadPool.SetMinThreads(500, completionPortThreads);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins"; // 参照：https://learn.microsoft.com/ja-jp/aspnet/core/security/cors?view=aspnetcore-7.0

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => {
    options.AddPolicy(name: MyAllowSpecificOrigins, policy => {
        policy.WithOrigins("http://localhost:3000")
              .AllowCredentials() // クレデンシャルを許可する
              .AllowAnyHeader(); // すべてのヘッダーを許可する
    });
});


//クッキー設定　いらんかったら後で消す　web apiでクッキーって使えるのかわからん
//セッションIDを手動で作ってアクション起こすときにそれをパラメータとして送ってもらうことにしよかなと思う今日この頃
//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromMinutes(5);
//    options.Cookie.HttpOnly = true;
//    options.Cookie.Name = "chat_app_test_cookie";

//});

//認証初期化　いらんかったら後で消す
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCookiePolicy();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors(MyAllowSpecificOrigins);

app.MapControllers();

app.Run();

