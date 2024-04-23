namespace CritterStackSample;

public record CreateCompany(string CompanyName);

public record CompanyCreated(Guid CompanyId, string CompanyName);

public class Company
{
   public Guid CompanyId { get; set; } 
   
   public string CompanyName { get; set; } = default!;
   
   public Company()
   {
   }
   
   public Company(CompanyCreated @event)
   {
      CompanyId = @event.CompanyId;
      CompanyName = @event.CompanyName;
   }
}