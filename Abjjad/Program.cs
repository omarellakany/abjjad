using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Abjjad.Common.Behaviors;
using Abjjad.Common.Middleware;
using Abjjad.Helpers;
using Abjjad.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddSingleton<ImageProcessingBackgroundService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ImageProcessingBackgroundService>());

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

builder.Services.AddSwaggerGen(c => { c.SchemaFilter<EnumSchemaFilter>(); });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => { b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseMiddleware<ErrorMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();