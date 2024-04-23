using JasperFx.Core;
using Marten;
using Wolverine;

namespace CritterStackSample;

public record CreateUser(string Name);

public record UserCreated(Guid UserId, Guid CompanyId, string Name);

public class UserHandler
{
    public async Task Handle(CreateUser command, IMessageBus bus, IDocumentStore store, CancellationToken ct)
    {
        var tenantId = CombGuidIdGeneration.NewGuid().ToString();

        var tenant = await store.Options.Tenancy.GetTenantAsync(tenantId);
        if (tenant is null)
        {
            throw new Exception("Tenant not found");
        }

        var created = await bus.InvokeForTenantAsync<CompanyCreated>(tenantId, new CreateCompany("Company Name"), ct);

        var @event = new UserCreated(CombGuidIdGeneration.NewGuid(), created.CompanyId, command.Name);

        await bus.PublishAsync(@event, new DeliveryOptions { TenantId = tenantId });
    }
}