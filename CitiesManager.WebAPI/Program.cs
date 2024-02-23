using CitiesManager.Core.Identity;
using CitiesManager.Core.Models;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Core.Services;
using CitiesManager.Infrastructure.DatabaseContext;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the  container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ProducesAttribute("application/json"));
    options.Filters.Add(new ConsumesAttribute("application/json"));

    //Authorization Policy
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
})
 .AddXmlSerializerFormatters();

//Adding the Jwt services 
builder.Services.AddTransient<IJwtService, JwtService>();
builder.Services.AddTransient<IEmailServices, EmailServices>();



//Enable versioning in Web API controllers
builder.Services.AddApiVersioning(config =>
{
    config.ApiVersionReader = new UrlSegmentApiVersionReader();
    //Reads version number from request url at "apiVersion" constraint
    //config.ApiVersionReader = new QueryStringApiVersionReader();
    //Reads version number from request query string called "API-version". EG:API-version=1.0
    //config.ApiVersionReader = new HeaderApiVersionReader("API-version"); 
    //Reads version number from request header called "API-version". EG: API-version: 1.0

    config.DefaultApiVersion = new ApiVersion(1, 0);
    config.AssumeDefaultVersionWhenUnspecified = true;
});




builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


//Swagger
builder.Services.AddEndpointsApiExplorer(); //Generates description for all endpoints


builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"));

    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Cities Web API", Version = "1.0" });

    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Cities Web API", Version = "2.0" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
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
                new string[] { }
           }
    });
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; //v1
    options.SubstituteApiVersionInUrl = true;
});

//CORS : localHost :4200
builder.Services.AddCors(options =>
{
    //Default CORS Policy
    options.AddDefaultPolicy(Policybuilder =>
    {
        Policybuilder
        .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>())
        .WithHeaders("Authorization", "origin", "accept", "content-type")
        .WithMethods("GET", "POST", "PUT", "DELETE");
    });

    //Custom CORS Policy
    options.AddPolicy("4100Client", Policybuilder =>
    {
        Policybuilder
        .WithOrigins(builder.Configuration.GetSection("AllowedOrigins2").Get<string[]>())
        .WithHeaders("Authorization", "origin", "accept")
        .WithMethods("GET");
    });

});

//Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 5;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.Tokens.EmailConfirmationTokenProvider = "Email";
}).AddEntityFrameworkStores<ApplicationDbContext>()
   .AddDefaultTokenProviders()
   .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
   .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();



//JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),


    };
});




builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrator", policy => policy.RequireRole("Administrator"));
    //  options.AddPolicy("User", policy => policy.RequireRole("User"));
    //    options.AddPolicy("Manager", policy => policy.RequireRole("Manager"));

    options.AddPolicy("ManagerAccess", policy =>
        policy.RequireAssertion(context =>
                    context.User.IsInRole("Administrator")
                    || context.User.IsInRole("Manager")));

    options.AddPolicy("UserAccess", policy =>
        policy.RequireAssertion(context =>

                     context.User.IsInRole("Manager")
                    || context.User.IsInRole("User")));

});

var app = builder.Build();

// Configure the HTTP request pipeline

app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSwagger(); //creates endpoint for swagger.json
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "2.0");
}); //creates swagger UI for testing all Web API endpoints / action methods

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
