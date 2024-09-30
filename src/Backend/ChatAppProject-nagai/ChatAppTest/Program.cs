using ChatAppTest;
using ChatAppTest.Controllers;
using ChatAppTest.FunctionController.Chat;
using ChatAppTest.FunctionController.Session;
using ChatAppTest.FunctionController.User;
using Microsoft.AspNetCore.Authentication.Cookies;

//���낢�돉�����@�^�C�}�[�Ƃ��N��
ChatController.Initialize();
ChatSessionController.Initialize();
ChatUserController.Initialize();
ChatReadCountController.Initialize();

//�X���b�h�v�[���ݒ�
ThreadPool.GetMinThreads(out var _, out var completionPortThreads);
ThreadPool.SetMinThreads(500, completionPortThreads);

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//CORS�ݒ�
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


//�N�b�L�[�ݒ�@����񂩂������ŏ����@web api�ŃN�b�L�[���Ďg����̂��킩���
//�Z�b�V����ID���蓮�ō���ăA�N�V�����N�����Ƃ��ɂ�����p�����[�^�Ƃ��đ����Ă��炤���Ƃɂ��悩�ȂƎv���������̍�
//builder.Services.AddDistributedMemoryCache();a
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromMinutes(5);
//    options.Cookie.HttpOnly = true;
//    options.Cookie.Name = "chat_app_test_cookie";

//});

//�F�؏������@����񂩂������ŏ���
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

