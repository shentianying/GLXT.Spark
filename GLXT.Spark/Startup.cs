using GLXT.Spark.AutoMapper;
using GLXT.Spark.Entity;
using GLXT.Spark.Filters;
using GLXT.Spark.IService;
using GLXT.Spark.Model;
using GLXT.Spark.Service;
using Hei.Captcha;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GLXT.Spark
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddControllers();
            services.AddControllers().AddNewtonsoftJson(option => {
                //格式化时间
                option.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                //忽略循环引用
                option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
            services.AddMemoryCache(); // 添加缓存
            services.AddSignalR();
            services.AddDirectoryBrowser();// 添加 目录浏览
            services.AddAutoMapper(typeof(AutoMapperConfigs));
            services.AddScoped<ICommonService, CommonService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();//上下文存储器
            services.AddSingleton<IPrincipalAccessor, PrincipalAccessor>();
            services.AddScoped<ISystemService, SystemService>();
            services.AddScoped<IBillFlowService, BillFlowService>();
            services.Configure<AppSettingModel>(Configuration.GetSection("AppSettings"));
            services.AddHeiCaptcha();
            #region JWT

            services.Configure<TokenManagement>(Configuration.GetSection("TokenManagement"));
            var token = Configuration.GetSection("TokenManagement").Get<TokenManagement>();
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token.Secret)),
                    ValidIssuer = token.Issuer,
                    ValidAudience = token.RefreshAudience,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    //ClockSkew默认值为5分钟，它是一个缓冲期,例如Token设置有效期为30分钟，
                    //到了30分钟的时候是不会过期的，会有这么个缓冲时间，也就是35分钟才会过期
                    //ClockSkew = TimeSpan.FromMinutes(1) 
                };
                x.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        //此处代码为终止.Net Core默认的返回类型和数据结果，这个很重要哦，必须
                        context.HandleResponse();

                        //自定义自己想要返回的数据结果，我这里要返回的是Json对象，通过引用Newtonsoft.Json库进行转换
                        var payload = JsonConvert.SerializeObject(new { code = 401, message = "很抱歉，您无权访问该接口或者token已经过期" });
                        //自定义返回的数据类型
                        context.Response.ContentType = "application/json";
                        //自定义返回状态码，默认为401 
                        //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        //输出Json数据结果
                        context.Response.WriteAsync(payload);
                        return Task.FromResult(0);
                    }
                };
            });
            #endregion

            #region 配置数据库
            services.AddDbContext<DBContext>(
                opt => opt.UseSqlServer(Configuration.GetConnectionString("Conn"))
            );
            #endregion

            #region 添加cors服务，配置跨域处理        
            services.AddCors(options =>
            {
                //策略1 
                options.AddPolicy("local", builder =>
                {
                    //builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();

                    builder.WithOrigins(
                        "http://localhost:9529",
                        "http://localhost:9530"
                        ).AllowAnyHeader()
                         .AllowAnyMethod();
                });
                //策略2 
                options.AddPolicy("any", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();

                    //builder.WithOrigins(
                    //    "http://localhost:9529",
                    //    "http://localhost:9530"
                    //    ).AllowAnyHeader()
                    //     .AllowAnyMethod();
                });
            });
            #endregion

            #region swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Spark接口 API", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, true); //添加控制器层注释（true表示显示控制器注释）

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme{
                                Reference = new OpenApiReference {
                                            Type = ReferenceType.SecurityScheme,
                                            Id = "Bearer"}
                           },new string[] { }
                        }
                    });

            });
            #endregion

            #region HangFire
            //services.AddHangfire(cfg =>
            //{
            //    cfg.UseSqlServerStorage(Configuration.GetConnectionString("Conn"));
            //});
            #endregion

            #region 设置全局过滤器
            services.AddMvc(option => option.Filters.Add(typeof(PermissionFilter)));
            services.AddControllers(option => option.Filters.Add(typeof(HttpResponseExceptionFilter)));
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error400");
            }
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseDirectoryBrowser();
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(
            //    Path.Combine(env.WebRootPath, "images")),
            //    RequestPath = "/MyImages"
            //});
            //app.UseDirectoryBrowser(new DirectoryBrowserOptions
            //{
            //    FileProvider = new PhysicalFileProvider(
            //        Path.Combine(env.WebRootPath, "images")),
            //    RequestPath = "/MyImages"
            //});

            app.UseCors("any");

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();
            #region swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                //string url = "/swagger/v1/swagger.json";
                string url = "v1/swagger.json";
                c.SwaggerEndpoint(url, "默认版本");
                c.DocExpansion(DocExpansion.None);//不展开
                //c.DefaultModelsExpandDepth(-1);// 不显示模型名称
            });
            #endregion

            #region hangfire
            //app.UseHangfireDashboard();
            //app.UseHangfireServer();
            #endregion

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapHub<MsgHub>("/msgHub");
            });
        }
    }
}
