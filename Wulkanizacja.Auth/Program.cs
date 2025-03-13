using Microsoft.Extensions.Options;
using Wulkanizacja.Auth.PostgreSQL.Context;
using Wulkanizacja.Auth.PostgreSQL.Options;
using Wulkanizacja.Auth.PostgreSQL.Repositories;
using Microsoft.EntityFrameworkCore;
using EntityFramework.Exceptions.PostgreSQL;
using Wulkanizacja.Auth.PostgreSQL.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDatabaseMigrationService, DatabaseMigrationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddDbContext<UserDbContext>((service, options) =>
                 {
                     var postgresOptions = service.GetRequiredService<IOptions<PostgresOptions>>().Value;
                     options
                         .UseNpgsql(postgresOptions.ConnectionString)
                         .UseExceptionProcessor();

                 });
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.Configure<PostgresOptions>(builder.Configuration.GetSection("postgres"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var migrationService = scope.ServiceProvider.GetRequiredService<IDatabaseMigrationService>();
    await migrationService.EnsureMigrationsAppliedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
