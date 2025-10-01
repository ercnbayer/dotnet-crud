using dotnetcrud.Data;
using dotnetcrud.Encryption;
using dotnetcrud.Middleware;
using dotnetcrud.Repositories;
using dotnetcrud.Security;
using dotnetcrud.Services;
using dotnetcrud.Swagger;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<AuthHeader>();
    c.AddSecurityDefinition("CustomAuth", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Enter your token here."
    });

});
builder.Services.Configure<Security.SecuritySettings>(
    builder.Configuration.GetSection("Security:SecuritySettings"));



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));





/*Dependency Injection  */
builder.Services.AddSingleton<EncryptionService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<Authenticate>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); //
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers(); //
//app.UseHttpsRedirection();



app.Run();

