using CritterStackSample;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Marten.Storage;
using Oakton;
using Weasel.Core;
using Wolverine;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ApplyOaktonExtensions();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddMarten(opts =>
    {
        opts.Connection(connectionString);
        opts.Policies.AllDocumentsAreMultiTenanted();
        opts.DisableNpgsqlLogging = true;

        // dotnet run -- marten-assert -i throws exceptions for this case
        opts.MultiTenantedWithSingleServer(connectionString);

        // dotnet run -- marten-assert -i shows only TenantsDatabases
        // opts.MultiTenantedDatabasesWithMasterDatabaseTable(x =>
        // {
        //     x.ConnectionString = connectionString;
        //     x.SchemaName = "test";
        //     x.AutoCreate = AutoCreate.CreateOrUpdate;
        //     x.ApplicationName = "CritterStackSample";
        // });

        // Without any multi tenancy it shows Marten and WolverineEnvelopeStorage database

        opts.Schema.For<Company>().Identity(x => x.CompanyId);
        opts.Projections.Snapshot<Company>(SnapshotLifecycle.Async);

        opts.DatabaseSchemaName = "test";
        opts.Events.DatabaseSchemaName = "test";

        opts.Events.TenancyStyle = TenancyStyle.Conjoined;
    }).AddAsyncDaemon(DaemonMode.Solo)
    .UseLightweightSessions()
    .IntegrateWithWolverine(schemaName: "test", transportSchemaName: "test", masterDatabaseConnectionString: connectionString);

builder.Host.UseWolverine(opts =>
{
    opts.Durability.Mode = DurabilityMode.Solo;
    opts.Policies.UseDurableLocalQueues();
    opts.Policies.AutoApplyTransactions();
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/test", async (IMessageBus bus) =>
    {
        await bus.InvokeAsync(new CreateUser("Test"));
        return Results.Ok();
    })
    .WithName("test")
    .WithOpenApi();

await app.RunOaktonCommands(args);