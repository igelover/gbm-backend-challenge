using Gbm.Challenge.API.Serialization;
using Gbm.Challenge.Application;
using Gbm.Challenge.Domain.Common.Settings;
using Gbm.Challenge.Infrastructure;
using System.Text.Json.Serialization;

namespace Gbm.Challenge.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            // Added to serialize enums as uppercase strings
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(new UpperCaseNamingPolicy()));
            // Added to comply with serialization requierement (snake case)
            options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();
        });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add custom services.
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddOpenApiDocumentation();
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.Configure<GbmChallengeSettings>(
            builder.Configuration.GetSection(nameof(GbmChallengeSettings))
        );

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.EnsureDatabaseSetup();
        }

        app.UseHttpsRedirection();

        app.UseOpenApiDocumentation();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapSmartAcControllers();
        app.Run();
    }
}