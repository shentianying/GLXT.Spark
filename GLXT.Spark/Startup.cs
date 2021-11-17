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
                //��ʽ��ʱ��
                option.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                //����ѭ������
                option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
            services.AddMemoryCache(); // ��ӻ���
            services.AddSignalR();
            services.AddDirectoryBrowser();// ��� Ŀ¼���
            services.AddAutoMapper(typeof(AutoMapperConfigs));
            services.AddScoped<ICommonService, CommonService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();//�����Ĵ洢��
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
                    //ClockSkewĬ��ֵΪ5���ӣ�����һ��������,����Token������Ч��Ϊ30���ӣ�
                    //����30���ӵ�ʱ���ǲ�����ڵģ�������ô������ʱ�䣬Ҳ����35���ӲŻ����
                    //ClockSkew = TimeSpan.FromMinutes(1) 
                };
                x.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        //�˴�����Ϊ��ֹ.Net CoreĬ�ϵķ������ͺ����ݽ�����������ҪŶ������
                        context.HandleResponse();

                        //�Զ����Լ���Ҫ���ص����ݽ����������Ҫ���ص���Json����ͨ������Newtonsoft.Json�����ת��
                        var payload = JsonConvert.SerializeObject(new { code = 401, message = "�ܱ�Ǹ������Ȩ���ʸýӿڻ���token�Ѿ�����" });
                        //�Զ��巵�ص���������
                        context.Response.ContentType = "application/json";
                        //�Զ��巵��״̬�룬Ĭ��Ϊ401 
                        //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        //���Json���ݽ��
                        context.Response.WriteAsync(payload);
                        return Task.FromResult(0);
                    }
                };
            });
            #endregion

            #region �������ݿ�
            services.AddDbContext<DBContext>(
                opt => opt.UseSqlServer(Configuration.GetConnectionString("Conn"))
            );
            #endregion

            #region ���cors�������ÿ�����        
            services.AddCors(options =>
            {
                //����1 
                options.AddPolicy("local", builder =>
                {
                    //builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();

                    builder.WithOrigins(
                        "http://localhost:9529",
                        "http://localhost:9530"
                        ).AllowAnyHeader()
                         .AllowAnyMethod();
                });
                //����2 
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
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Spark�ӿ� API", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, true); //��ӿ�������ע�ͣ�true��ʾ��ʾ������ע�ͣ�

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "���¿�����������ͷ����Ҫ���Jwt��ȨToken��Bearer Token",
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

            #region ����ȫ�ֹ�����
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
                c.SwaggerEndpoint(url, "Ĭ�ϰ汾");
                c.DocExpansion(DocExpansion.None);//��չ��
                //c.DefaultModelsExpandDepth(-1);// ����ʾģ������
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
