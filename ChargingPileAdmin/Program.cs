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

// 添加仓储
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IChargingStationRepository, ChargingStationRepository>();
builder.Services.AddScoped<IChargingPileRepository, ChargingPileRepository>();
builder.Services.AddScoped<IChargingPortRepository, ChargingPortRepository>();

// 添加服务
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IUserAddressService, UserAddressService>();
builder.Services.AddScoped<IUserNotificationService, UserNotificationService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IChargingStationService, ChargingStationService>();
builder.Services.AddScoped<IChargingPileService, ChargingPileService>();
builder.Services.AddScoped<IChargingPortService, ChargingPortService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();

// 添加控制器
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 使用camelCase命名约定，确保属性名首字母小写
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        // 忽略空值
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// 添加Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "充电桩后台管理API", Version = "v1" });

    // 设置XML文档路径
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // 启用注释中的Swagger注释
    c.EnableAnnotations();
});

// 添加CORS服务
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

var app = builder.Build();

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "充电桩后台管理API v1"));
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

// 添加静态文件中间件
// 配置默认文件选项，将login.html设置为默认页面
var defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("login.html");
app.UseDefaultFiles(defaultFilesOptions);
app.UseStaticFiles(); // 启用静态文件服务

// 启用CORS
app.UseCors("AllowAll");

// HTTPS重定向（如果遇到问题可以注释掉）
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// 启动时执行数据库脚本，自动修复数据库缺少的列
try
{
    // 获取连接字符串
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // 读取SQL脚本内容
    var sqlPilesScript = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "add_description_to_piles.sql"));
    var sqlStationsScript = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "add_description_column.sql"));
    
    // 执行SQL脚本
    using (var connection = new SqlConnection(connectionString))
    {
        connection.Open();
        
        // 执行充电站描述列脚本
        using (var command = new SqlCommand(sqlStationsScript, connection))
        {
            command.ExecuteNonQuery();
        }
        
        // 执行充电桩描述列脚本
        using (var command = new SqlCommand(sqlPilesScript, connection))
        {
            command.ExecuteNonQuery();
        }
        
        connection.Close();
    }
    
    Console.WriteLine("数据库列修复脚本执行成功！");
}
catch (Exception ex)
{
    Console.WriteLine($"执行数据库修复脚本时出错: {ex.Message}");
}

app.Run();
