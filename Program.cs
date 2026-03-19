using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OptiControl.Data;
using OptiControl.Services;
using OptiControl.Services.IServices;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    // Evita que propiedades de navegación no nullable (ej. Reservation.Client) se validen como requeridas al recibir solo el ID.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// JWT (no cookies ni sesión)
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSecret = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey not configured.");
var jwtIssuer = jwtSection["Issuer"] ?? "OptiControl";
var jwtAudience = jwtSection["Audience"] ?? "OptiControlUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = _ => Task.CompletedTask,
            OnChallenge = context =>
            {
                // Añadir CORS a la respuesta 401 para que el frontend reciba el error (si no, el navegador reporta CORS)
                var origin = context.Request.Headers.Origin.FirstOrDefault();
                var allowed = new[]
                {
                    "http://localhost:5173",
                    "http://localhost:3000",
                    "https://opticontrol.cowib.es",
                    "https://opticontrol-frontend.cowib.es",
                };
                if (!string.IsNullOrEmpty(origin) && allowed.Contains(origin, StringComparer.OrdinalIgnoreCase))
                {
                    context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
                    context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
                }
                context.HandleResponse();
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireClaim("Rol", OptiControl.Utils.SD.RolAdministrador));
    options.AddPolicy("Usuario", policy => policy.RequireClaim("Rol", OptiControl.Utils.SD.RolUsuario, OptiControl.Utils.SD.RolAdministrador));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "https://opticontrol.cowib.es",
                "https://opticontrol-frontend.cowib.es",
                "https://aventours.cowib.es",
                "https://trippilot.cowib.es",
                "https://loading-aventours.cowib.es")
                
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IInvoicePdfService, InvoicePdfService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<ICajaService, CajaService>();
builder.Services.AddScoped<IWhatsAppTemplateService, WhatsAppTemplateService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IWebsiteServiceService, WebsiteServiceService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IServiceOpticaService, ServiceOpticaService>();
builder.Services.AddScoped<IOpticsSaleService, OpticsSaleService>();
builder.Services.AddScoped<IDashboardOpticsService, DashboardOpticsService>();

var app = builder.Build();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var pending = db.Database.GetPendingMigrations().ToList();
        if (pending.Count > 0)
            db.Database.Migrate();
        InicializarUsuarioAdmin.CrearAdminSiNoExiste(db, logger);
        SeedAgencySettingsIfEmpty(db);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al inicializar la base de datos");
    }
}

app.UseCors();
app.UseStaticFiles(); // wwwroot (p. ej. /images/logo.png)
if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static void SeedAgencySettingsIfEmpty(ApplicationDbContext db)
{
    if (db.AgencySettings.Any()) return;
    db.AgencySettings.Add(new OptiControl.Models.Entities.AgencySettings
    {
        CompanyName = "OptiControl",
        Currency = "NIO",
        Language = "es",
        ExchangeRate = 36.8m,
        Theme = "light",
        SoundVolume = 80,
        AlertsReservacionesPendientes = true,
        AlertsFacturasVencidas = true,
        AlertsRecordatorios = true
    });
    db.SaveChanges();
}
