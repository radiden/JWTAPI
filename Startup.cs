using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using jwtapi.Data;
using jwtapi.Models;
using Microsoft.AspNetCore.Identity;

namespace jwtapi
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

            services.AddControllers();
            services.AddDbContext<RefreshContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("RefreshContext"),
                    new MariaDbServerVersion(new Version(10, 5, 9))));
            services.AddDbContext<UserContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("RefreshContext"),
                    new MariaDbServerVersion(new Version(10, 5, 9))));
            services.AddDbContext<ClientContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("ProductContext"),
                    new MariaDbServerVersion(new Version(10, 5, 9))));
            services.AddDbContext<ProductContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("ProductContext"),
                    new MariaDbServerVersion(new Version(10, 5, 9))));
            services.AddDbContext<TransactionContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("ProductContext"),
                    new MariaDbServerVersion(new Version(10, 5, 9))));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "jwtapi", Version = "v1" });
            });
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer", "Manager", "Admin"));
                options.AddPolicy("RequireManagerRole", policy => policy.RequireRole("Manager", "Admin"));
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
            });

            services.AddIdentity<UserModel, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredUniqueChars = 0;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<UserContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = "jwtapi",
                    ValidAudience = "jwtapiuser",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("JWTSecret"))),
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "jwtapi v1"));
            }

            // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
