using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventsDemo.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Cors.Infrastructure;
using EventsDemo.CustomFormatter;
using EventsDemo.CustomFilters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventsDemo
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
           services.AddDbContext<EventDbContext>(options =>
           {
               options.UseInMemoryDatabase(databaseName: "EventDb");
               //options.UseSqlServer(Configuration.GetConnectionString("EventSqlConnection"));              
           });

            /*services.AddCors(c =>
            {
                CorsPolicy policy = new CorsPolicy();
                policy.Origins.Add("*.microsoft.com");              
                c.AddPolicy("MSPolicy", policy);
             });*/

           services.AddCors(c =>
           {
               c.AddPolicy("MSPolicy", builder =>
               {
                   builder.WithOrigins("*.microsoft.com")
                           .AllowAnyMethod()
                           .AllowAnyHeader();
               });
               c.AddPolicy("SynPolicy", builder =>
               {
                   builder.WithOrigins("*.synergetics-india.com")
                           .WithMethods("GET")
                           .WithHeaders("Authorization", "Content-Type","Accept");
               });
               c.AddPolicy("Others", builder =>
               {
                   builder.AllowAnyOrigin()
                           .WithMethods("GET")
                           .WithHeaders("Authorization", "Content-Type");
               });
               c.DefaultPolicyName = "Others"; //If the policy doesnt match the first two, "Others" would be taken as default.
           });

           services.AddSwaggerGen(c=>
           {
                c.SwaggerDoc("EventAPI", new Info
                {
                    Title = "Event API",
                    Version = "v1",
                    Contact = new Contact { Name = "Unais Kamle", Email = "unaiskamle@gmail.com" }
                });

           });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = Configuration.GetValue<string>("Jwt:Issuer"),
                       ValidAudience = Configuration.GetValue<string>("Jwt:Audience"),
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("Jwt:Secret")))
                   };
               });

            // services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMvc(c=>
            {
                c.Filters.Add(typeof(CustomExceptionHandler)); //Adding the custom exception handler
                c.OutputFormatters.Add(new CsvCustomFormatter());//Added custom csv formatter
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            /*app.UseCors(c => //Use Cors middleware and configure policies
            {
                c.WithOrigins("*.microsoft.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                c.WithOrigins("*.synergetics-india.com")
                    .WithMethods("GET")
                    .WithHeaders("Authorization","Content-Type","Accept");
            });*/


            app.UseAuthentication();

            app.UseCors();

            InitializeDatabase(app);

            app.UseSwagger();
            //if (env.IsDevelopment())
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/EventAPI/swagger.json", "Event API list");
                });
            //app.UseHttpsRedirection();
            app.UseMvc();
        }

        public void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<EventDbContext>();

                db.Events.Add(new Models.EventInfo
                {
                    Title = "Sample Event 1",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(2),
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddTicks(10000000),
                    Host ="Microsoft",
                    Speaker = "Unais K",
                    RegistrationUrl = "https://events.microsoft.com/1224"
                    
                });

                db.Events.Add(new Models.EventInfo
                {
                    Title = "Sample Event 2",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(2),
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddTicks(10000000),
                    Host = "Microsoft",
                    Speaker = "Unais K",
                    RegistrationUrl = "https://events.microsoft.com/1225"

                });
                db.SaveChanges();
            }
        }
    }
}
