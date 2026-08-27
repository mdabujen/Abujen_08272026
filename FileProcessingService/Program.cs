using FileProcessingService.Middleware;
using FileProcessingService.Services;
using FileProcessingService.Services.Interfaces;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


builder.Services.AddScoped<
    ICsvProcessingService, CsvProcessingService>();
builder.Services.AddSingleton<
    IFileTrackingService, FileTrackingService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "File Processing API",
        Version = "v1",
        Description = "Uploads CSV files, computes a column average, and tracks processing history"
    });

    options.AddSecurityDefinition("ApiKey",
        new OpenApiSecurityScheme
        {
            Name = "X-Api-Key",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Description = "API key required for all endpoints. Example: X-Api-Key: your-key-here"
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
    {
            [new OpenApiSecuritySchemeReference("ApiKey", document)] =
                new List<string>()
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();
