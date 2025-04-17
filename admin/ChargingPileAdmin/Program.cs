#nullable disable

using ChargingPileAdmin.Data;
using ChargingPileAdmin.Repositories;
using ChargingPileAdmin.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using System.IO;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// 添加数据库上下文
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.UseNetTopologySuite())); // 启用NetTopologySuite支持地理空间数据

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // 添加全局异常处理中间件（仅在非开发环境中使用）
    app.UseExceptionHandler(appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;
            
            // 创建标准错误响应对象
            var errorResponse = new 
            {
                status = 500,
                message = "服务器发生错误",
                error = exception?.Message ?? "未知错误",
                path = context.Request.Path
            };
            
            // 序列化为JSON并返回
            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            
            await context.Response.WriteAsync(jsonResponse);
        });
    });
}

app.Run();