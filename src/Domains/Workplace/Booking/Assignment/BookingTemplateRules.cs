namespace Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>A rule a booking template must pass. Check returns its problems, or none.</summary>
public interface IBookingTemplateRule
{
    List<string> Check(BookingTemplate item);
}

public sealed record BookingTemplateCeilingRangeRule(double Min, double Max) : IBookingTemplateRule
{
    public List<string> Check(BookingTemplate item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BookingTemplateRequiredCodeRule(string Message) : IBookingTemplateRule
{
    public List<string> Check(BookingTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BookingTemplateRequiredTeamRule(string Message) : IBookingTemplateRule
{
    public List<string> Check(BookingTemplate item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a booking template.</summary>
public sealed record BookingTemplateValidator(List<IBookingTemplateRule> Rules)
{
    public List<string> Validate(BookingTemplate item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(BookingTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BookingTemplate> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of booking template rules.</summary>
public static class BookingTemplateRules
{
    public static BookingTemplateValidator DefaultBookingTemplateValidator()
    {
        return new BookingTemplateValidator(
            new List<IBookingTemplateRule>
            {
                new BookingTemplateCeilingRangeRule(6.2, 1026.4),
                new BookingTemplateRequiredCodeRule("booking template needs an entry"),
                new BookingTemplateRequiredTeamRule("booking template needs this field"),
            });
    }
}
