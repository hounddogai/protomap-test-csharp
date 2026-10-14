namespace Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>A rule a seasonal booking template must pass. Check returns its problems, or none.</summary>
public interface ISeasonalBookingTemplateRule
{
    List<string> Check(SeasonalBookingTemplate item);
}

public sealed record SeasonalBookingTemplateRequiredCodeRule(string Message) : ISeasonalBookingTemplateRule
{
    public List<string> Check(SeasonalBookingTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalBookingTemplateTenureMonthsRangeRule(int Min, int Max) : ISeasonalBookingTemplateRule
{
    public List<string> Check(SeasonalBookingTemplate item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalBookingTemplateRequiredSegmentRule(string Message) : ISeasonalBookingTemplateRule
{
    public List<string> Check(SeasonalBookingTemplate item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal booking template.</summary>
public sealed record SeasonalBookingTemplateValidator(List<ISeasonalBookingTemplateRule> Rules)
{
    public List<string> Validate(SeasonalBookingTemplate item)
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

    public void RequireValid(SeasonalBookingTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalBookingTemplate> items)
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

/// <summary>Functions of seasonal booking template rules.</summary>
public static class SeasonalBookingTemplateRules
{
    public static SeasonalBookingTemplateValidator DefaultSeasonalBookingTemplateValidator()
    {
        return new SeasonalBookingTemplateValidator(
            new List<ISeasonalBookingTemplateRule>
            {
                new SeasonalBookingTemplateRequiredCodeRule("seasonal booking template needs this field"),
                new SeasonalBookingTemplateTenureMonthsRangeRule(80, 120),
                new SeasonalBookingTemplateRequiredSegmentRule("seasonal booking template needs an entry"),
            });
    }
}
