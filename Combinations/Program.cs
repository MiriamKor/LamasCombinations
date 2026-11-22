using Combinations.Middleware;
using Combinations.Models.Settings;
using Combinations.Services;
using Combinations.Sessions;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CombinationSettings>(
    builder.Configuration.GetSection(CombinationSettings.SectionName));

builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IPermutationAlgorithms, PermutationAlgorithms>();
builder.Services.AddSingleton<ICombinationService, CombinationService>(); // stateless
builder.Services.AddSessionStore(builder.Configuration); // provider chosen via "SessionStore:Provider"
builder.Services.AddSingleton<ISessionManager, SessionManager>();
builder.Services.AddScoped<ICombinationSession, CombinationSession>();
builder.Services.AddHostedService<SessionCleanupService>();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Preferred: use the Angular dev-server proxy (same origin). Otherwise list the Angular
// origin(s) under "Cors:AllowedOrigins" and the session cookie works cross-origin.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
}

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
if (allowedOrigins.Length > 0)
    app.UseCors();
app.UseRateLimiter();

app.MapControllers().RequireRateLimiting("api");

app.Run();


