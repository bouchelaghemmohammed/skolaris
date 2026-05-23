using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Services;


var builder = WebApplication.CreateBuilder(args);

// Enum conversion
builder.Services.AddControllers(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters
        .Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddScoped<AbsenceService>();
builder.Services.AddScoped<InscriptionService>();
builder.Services.AddScoped<NoteService>();
builder.Services.AddScoped<BulletinService>();
builder.Services.AddScoped<GrilleEvaluationService>();
builder.Services.AddScoped<MessagerieService>();
builder.Services.AddScoped<NotificationService>();

// Limite upload: 10 Mo pour les pièces jointes de la messagerie
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10_485_760; // 10 Mo
});
builder.WebHost.ConfigureKestrel(opts =>
{
    opts.Limits.MaxRequestBodySize = 10_485_760; // 10 Mo
});


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

// mohammed ! création automatique de la BD + seed au démarrage ne le change pas svp !!!!!!
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated(); // crée toutes les tables depuis le DbContext, sans fichiers migration
    DataSeeder.Seed(db);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowBlazor");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();
app.MapControllers();



app.Run();
