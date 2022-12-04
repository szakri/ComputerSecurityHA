using Backend.Services;
using DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Text;

namespace Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<CaffContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            Mapper.SetHeasher(builder.Configuration["Hash:Salt"], int.Parse(builder.Configuration["Hash:MinLength"]));

#if DEBUG
            builder.Services.AddControllers();
#else
			builder.Services.AddControllers().ConfigureApplicationPartManager(mgr =>
			{
				mgr.FeatureProviders.Clear();
				mgr.FeatureProviders.Add(new MyFeatureProvider());
			});
#endif
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "Computer Security HA",
					Description = "Web API for the purchasing of CAFF files.",
				});
				var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
			});

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                    ClockSkew = TimeSpan.Zero,
                    LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken token, TokenValidationParameters @params) =>
                    {
                        if (expires != null)
                        {
                            return expires > DateTime.UtcNow;
                        }
                        return false;
                    }
                };
            });
            builder.Services.AddAuthorization();

            var app = builder.Build();

			app.UseStatusCodePagesWithReExecute("/");

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHttpsRedirection();

			app.UseDefaultFiles();
			app.UseStaticFiles();

			app.MapControllers();

            app.Run();
        }

		public class MyFeatureProvider : ControllerFeatureProvider
		{
			protected override bool IsController(TypeInfo typeInfo)
			{
				if (typeInfo.Name == "TestController")
				{
					return false;
				}
				return base.IsController(typeInfo);
			}
		}
	}
}