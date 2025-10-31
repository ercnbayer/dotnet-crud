using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using dotnetcrud.Data;
using dotnetcrud.Encryption;
using dotnetcrud.Middleware;
using dotnetcrud.Repositories;
using dotnetcrud.S3;
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
    // Security scheme
    c.AddSecurityDefinition("CustomAuth", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Enter your token here."
    });

    // Apply to endpoints
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "CustomAuth"
                }
            },
            []
        }
    });

});

// S3 Configuration
var s3Config = new S3Config
{
    ServiceUrl = builder.Configuration["AWS:ServiceURL"] ?? "http://localhost:4566",
    AccessKey = builder.Configuration["AWS:AccessKey"] ?? "test",
    SecretKey = builder.Configuration["AWS:SecretKey"] ?? "test",
    BucketName = builder.Configuration["AWS:BucketName"] ?? "my-bucket",
    Region = builder.Configuration["AWS:Region"] ?? "us-east-1",
    QueueName = builder.Configuration["AWS:QueueName"] ?? "my-queue",
};


builder.Services.AddSingleton(s3Config);

// S3 client register as scoped
builder.Services.AddScoped<IAmazonS3>(sp =>
{
    var config = sp.GetRequiredService<S3Config>();
    var credentials = new BasicAWSCredentials(config.AccessKey, config.SecretKey);
    var awsConfig = new AmazonS3Config
    {
        ServiceURL = config.ServiceUrl,
        ForcePathStyle = true,
        AuthenticationRegion = config.Region,
        RegionEndpoint = RegionEndpoint.USEast1
    };

    return new AmazonS3Client(credentials, awsConfig);
});

// S3 and File services
builder.Services.AddScoped<IS3Service, S3Service>();
builder.Services.AddScoped<IFileRepository, FileRepository>();

// AWS SQS and Background Worker
builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    var awsConfig = new AmazonSQSConfig
    {
        ServiceURL = s3Config.ServiceUrl, // http://localhost:4566
        AuthenticationRegion = s3Config.Region
    };
    var credentials = new BasicAWSCredentials(s3Config.AccessKey, s3Config.SecretKey);
    return new AmazonSQSClient(credentials, awsConfig);
});

builder.Services.AddHostedService<S3EventWorker>();

// Configure Security Settings
builder.Services.Configure<Security.SecuritySettings>(
    builder.Configuration.GetSection("Security:SecuritySettings"));


// Database Connection (using Npgsql/PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


/* Cleaned and Refactored Dependency Injection Section */

// EncryptionService registered as Singleton for IEncryptionService 
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

// User Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Middleware Class
builder.Services.AddScoped<Authenticate>();


var app = builder.Build();

// Apply database migrations on application startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();
//app.UseHttpsRedirection();



app.Run();
