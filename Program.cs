using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<EtablissementService>();
builder.Services.AddScoped<SessionAcademiqueService>();
builder.Services.AddScoped<DossierAcademiqueService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Migration automatique + seed au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    DataSeeder.Seed(db);
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowBlazor");
app.UseAuthorization();
app.MapControllers();

app.Run();
