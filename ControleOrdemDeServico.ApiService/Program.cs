using Microsoft.AspNetCore.Diagnostics;
using OsService.Infrastructure.Databases;
using OsService.Infrastructure.Repository;
using OsService.Services.Exceptions;
using OsService.Services.V1.Customers.CreateCustomer;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers()
.AddJsonOptions(options =>
 {
     options.JsonSerializerOptions.Converters.Add(
         new JsonStringEnumConverter());
 });

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateCustomerCommand).Assembly));

var defaultCs = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var adminCs = builder.Configuration.GetConnectionString("CreateTable")
    ?? throw new InvalidOperationException("Connection string 'CreateTable' not found.");

builder.Services.AddSingleton<IDefaultSqlConnectionFactory>(_ =>
    new SqlConnectionFactory(defaultCs));

builder.Services.AddSingleton<IAdminSqlConnectionFactory>(_ =>
    new SqlConnectionFactory(adminCs));

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
builder.Services.AddScoped<IServiceOrderAttachmentRepository, ServiceOrderAttachmentRepository>();

builder.Services.AddSingleton<DatabaseGenerantor>();

builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var generator = scope.ServiceProvider.GetRequiredService<DatabaseGenerantor>();
    await generator.EnsureCreatedAsync(CancellationToken.None);
}

app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var ex = feature?.Error;

        var status = ex switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            ConflictException => StatusCodes.Status409Conflict,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var errorMessage = status == StatusCodes.Status500InternalServerError
        ? "An unexpected error occurred while processing your request."
        : ex?.Message;

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = errorMessage
        });
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OsService API v1");
        c.RoutePrefix = "swagger";
    });

    app.MapOpenApi();
}

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();