using ChatAppTest;
using ChatAppTest.Controllers;
using ChatAppTest.FunctionController.Chat;
using ChatAppTest.FunctionController.Session;
using ChatAppTest.FunctionController.User;
using Microsoft.AspNetCore.Authentication.Cookies;

//いろいろ初期化　タイマーとか起動
ChatController.Initialize();
ChatSessionController.Initialize();
ChatUserController.Initialize();
ChatReadCountController.Initialize();

//スレッドプール設定
ThreadPool.GetMinThreads(out var _, out var completionPortThreads);
ThreadPool.SetMinThreads(500, completionPortThreads);

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//CORS設定
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins,
        builder =>
        {
            builder.WithOrigins("http://localhost:3000");
            builder.AllowAnyMethod();
            builder.AllowCredentials();
            builder.AllowAnyHeader();
        });
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//クッキー設定　いらんかったら後で消す　web apiでクッキーって使えるのかわからん
//セッションIDを手動で作ってアクション起こすときにそれをパラメータとして送ってもらうことにしよかなと思う今日この頃
//builder.Services.AddDistributedMemoryCache();a
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

app.UseCors(policyName:MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

