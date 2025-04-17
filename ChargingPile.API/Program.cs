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
using System.Net.Http;
using System.Collections.Specialized;
using ChargingPile.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 配置Kestrel服务器使用固定端口
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls("http://localhost:5065");
}

// 添加基本控制器
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 允许属性名称不区分大小写
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        // 允许属性名称不完全匹配
        options.JsonSerializerOptions.AllowTrailingCommas = true;
        // 忽略注释
        options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
        // 包含空值
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
    })
    // 禁用模型验证
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

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

// 获取Swagger配置
var swaggerSettings = builder.Configuration.GetSection("SwaggerSettings");
var swaggerEnabled = swaggerSettings.GetValue<bool>("Enabled", false);

// 配置Swagger
if (swaggerEnabled || builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = swaggerSettings.GetValue<string>("Title", "充电桩管理系统"),
            Version = swaggerSettings.GetValue<string>("Version", "v1"),
            Description = swaggerSettings.GetValue<string>("Description", "充电桩管理系统API")
        });

        // 设置XML文档路径
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        // 添加JWT授权按钮
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "请输入JWT令牌，格式为: Bearer {token}",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
}

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
    var configuration = sp.GetRequiredService<IConfiguration>();
    var dbConnection = sp.GetRequiredService<IDbConnection>();
    return new TcpServer(logger, configuration, dbConnection);
});

// 注册充电桩服务
builder.Services.AddScoped<ChargingPileService>();

// 注册充电站服务
builder.Services.AddScoped<StationService>();

// 注册JWT服务
builder.Services.AddScoped<JwtService>();

// 注册用户服务
builder.Services.AddScoped<UserService>();

// 注册微信支付服务
builder.Services.AddScoped<WechatPayService>();

// 注册支付服务
builder.Services.AddScoped<PaymentService>();

// 添加日志服务
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 添加HttpClient
builder.Services.AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName, client => { })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    })
    .AddHttpMessageHandler(() => new SensitiveDataLoggingHandler());

// 添加HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// 配置JWT认证
var jwtSettings = builder.Configuration.GetSection("JWT");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
});


var app = builder.Build();

// 配置中间件
// 在开发环境或配置中启用时启用Swagger
if (app.Environment.IsDevelopment() || swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "充电桩管理系统 V1");
        c.RoutePrefix = "swagger";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // 默认折叠所有接口
        c.EnablePersistAuthorization(); // 启用授权信息持久化

        // 添加自定义JS脚本，实现自动保存和应用Token
        c.HeadContent = @"
            <script>
                window.onload = function() {
                    // 从登录响应中提取并保存token
                    var originalFetch = window.fetch;
                    window.fetch = function(url, options) {
                        return originalFetch(url, options).then(function(response) {
                            var clonedResponse = response.clone();
                            if (url.includes('/api/user/login') || url.includes('/api/user/register')) {
                                clonedResponse.json().then(function(data) {
                                    if (data && data.data && data.data.token) {
                                        localStorage.setItem('swagger_token', data.data.token);
                                        // 自动填充授权信息
                                        setTimeout(function() {
                                            var authBtn = document.querySelector('.authorize');
                                            if (authBtn) {
                                                authBtn.click();
                                                setTimeout(function() {
                                                    var tokenInput = document.querySelector('.auth-container input');
                                                    var authorizeBtn = document.querySelector('.auth-btn-wrapper button.authorize');
                                                    if (tokenInput && authorizeBtn) {
                                                        tokenInput.value = 'Bearer ' + data.data.token;
                                                        authorizeBtn.click();
                                                        setTimeout(function() {
                                                            document.querySelector('.btn-done').click();
                                                        }, 500);
                                                    }
                                                }, 500);
                                            }
                                        }, 1000);
                                    }
                                });
                            }
                            return response;
                        });
                    };

                    // 页面加载时尝试从本地存储中恢复令牌
                    setTimeout(function() {
                        var savedToken = localStorage.getItem('swagger_token');
                        if (savedToken) {
                            var authBtn = document.querySelector('.authorize');
                            if (authBtn) {
                                authBtn.click();
                                setTimeout(function() {
                                    var tokenInput = document.querySelector('.auth-container input');
                                    var authorizeBtn = document.querySelector('.auth-btn-wrapper button.authorize');
                                    if (tokenInput && authorizeBtn) {
                                        tokenInput.value = 'Bearer ' + savedToken;
                                        authorizeBtn.click();
                                        setTimeout(function() {
                                            document.querySelector('.btn-done').click();
                                        }, 500);
                                    }
                                }, 500);
                            }
                        }
                    }, 1000);
                };
            </script>
        ";
    });
}

app.UseHttpsRedirection();

// 确保CORS中间件在正确的位置注册
app.UseCors("AllowAll");

// 添加认证中间件
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 启动TCP服务器
var tcpServer = app.Services.GetRequiredService<TcpServer>();
_ = tcpServer.StartAsync(app.Lifetime.ApplicationStopping);

app.Run();

// 敏感信息过滤处理类
public class SensitiveDataLoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 处理请求前的日志记录，对URL中的敏感信息进行脱敏
        var requestUri = request.RequestUri?.ToString() ?? "";
        var maskedUri = MaskSensitiveData(requestUri);

        // 记录脱敏后的请求URL
        Console.WriteLine($"HTTP Request: {request.Method} {maskedUri}");

        // 发送请求
        var response = await base.SendAsync(request, cancellationToken);

        // 记录响应状态码，不记录响应内容（可能包含敏感信息）
        Console.WriteLine($"HTTP Response: {(int)response.StatusCode} {response.StatusCode}");

        return response;
    }

    private string MaskSensitiveData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return data;

        // 对包含 secret 参数的URL进行脱敏
        if (data.Contains("secret="))
        {
            var uri = new Uri(data);
            // 使用 NameValueCollection 替代 ParseQueryString
            var query = new NameValueCollection();

            // 手动解析查询参数
            string queryString = uri.Query.TrimStart('?');
            string[] pairs = queryString.Split('&');
            foreach (string pair in pairs)
            {
                if (string.IsNullOrEmpty(pair)) continue;

                string[] keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = Uri.UnescapeDataString(keyValue[0]);
                    string value = Uri.UnescapeDataString(keyValue[1]);
                    query.Add(key, value);
                }
            }

            // 检查并替换敏感参数
            if (query["secret"] != null)
            {
                query["secret"] = "***";
            }

            // 重建查询字符串
            var newQueryString = new StringBuilder();
            foreach (string key in query.Keys)
            {
                if (newQueryString.Length > 0)
                    newQueryString.Append('&');

                newQueryString.Append(Uri.EscapeDataString(key));
                newQueryString.Append('=');
                newQueryString.Append(Uri.EscapeDataString(query[key]));
            }

            // 重建 URL
            var uriBuilder = new UriBuilder(uri);
            uriBuilder.Query = newQueryString.ToString();
            return uriBuilder.Uri.ToString();
        }

        return data;
    }
}
