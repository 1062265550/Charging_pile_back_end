#nullable disable

using ChargingPileAdmin.Data;
using ChargingPileAdmin.Repositories;
using ChargingPileAdmin.Services;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;

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

// 添加静态文件中间件
app.UseDefaultFiles(); // 允许默认访问index.html
app.UseStaticFiles(); // 启用静态文件服务

// 启用CORS
app.UseCors("AllowAll");

// HTTPS重定向（如果遇到问题可以注释掉）
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
