using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ChargingPile.API.Communication;
using ChargingPile.API.Services;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using ChargingPile.API.Data;

var builder = WebApplication.CreateBuilder(args);

// 添加基本控制器
builder.Services.AddControllers();

// 配置跨域策略
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// 配置Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "充电桩管理系统",
        Version = "v1",
        Description = "充电桩管理系统API"
    });

    // 设置XML文档路径
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// 注册IDbConnection
builder.Services.AddTransient<IDbConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("数据库连接字符串未配置");
    }
    
    // 使用SQL Server连接
    var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
    connection.Open();
    return connection;
});

// 配置TCP服务器
builder.Services.AddSingleton<TcpServer>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<TcpServer>>();
    return new TcpServer(logger);
});

// 注册充电桩服务
builder.Services.AddScoped<ChargingPileService>();

var app = builder.Build();

// 配置中间件
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "充电桩管理系统 V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 启动TCP服务器
var tcpServer = app.Services.GetRequiredService<TcpServer>();
_ = tcpServer.StartAsync(app.Lifetime.ApplicationStopping);

app.Run();
