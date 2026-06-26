using Microsoft.EntityFrameworkCore;
using Orbit.Actions;
using Orbit.Engine;
using Orbit.Persistence;

var workflowsDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "workflows");
var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Orbit", "orbit.db");

Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOrbitPersistence(dbPath)
    .AddOrbitActions()
    .AddOrbitEngine(workflowsDir)
    .AddHostedService<Worker>();

var host = builder.Build();

// Apply EF migrations on startup
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Orbit.Persistence.OrbitDbContext>();
    db.Database.Migrate();
}

host.Run();
