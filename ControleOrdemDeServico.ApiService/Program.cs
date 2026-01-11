using Microsoft.AspNetCore.Diagnostics;
using OsService.Infrastructure.Databases;
using OsService.Infrastructure.Repository;
using OsService.Services.Exceptions;
using OsService.Services.V1.Customers.CreateCustomer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateCustomerCommand).Assembly));

builder.Services.AddSingleton<IDefaultSqlConnectionFactory>(_ =>
    new SqlConnectionFactory(
        builder.Configuration.GetConnectionString("ConnectionStrings__DefaultConnection")!));

builder.Services.AddSingleton<IAdminSqlConnectionFactory>(_ =>
    new SqlConnectionFactory(
        builder.Configuration.GetConnectionString("ConnectionStrings__CreateTable")!));

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();

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

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = ex?.Message
        });
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();