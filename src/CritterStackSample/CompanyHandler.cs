using Marten;

namespace CritterStackSample;

public class CompanyHandler
{
    public object Handle(CreateCompany command, IDocumentSession session)
    {
        var companyId = Guid.Parse(session.TenantId);
        var @event = new CompanyCreated(companyId, command.CompanyName);
        session.Events.StartStream<Company>(companyId, @event);

        return @event;
    }
}