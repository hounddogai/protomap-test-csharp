namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

/// <summary>A rule a engagement request must pass. Check returns its problems, or none.</summary>
public interface IEngagementRequestRule
{
    List<string> Check(EngagementRequest item);
}

public sealed record EngagementRequestCodeLengthRule(int MaxLength) : IEngagementRequestRule
{
    public List<string> Check(EngagementRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementRequestCapacityRangeRule(int Min, int Max) : IEngagementRequestRule
{
    public List<string> Check(EngagementRequest item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EngagementRequestHoursRangeRule(double Min, double Max) : IEngagementRequestRule
{
    public List<string> Check(EngagementRequest item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement request.</summary>
public sealed record EngagementRequestValidator(List<IEngagementRequestRule> Rules)
{
    public List<string> Validate(EngagementRequest item)
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

    public void RequireValid(EngagementRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementRequest> items)
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

/// <summary>Functions of engagement request rules.</summary>
public static class EngagementRequestRules
{
    public static EngagementRequestValidator DefaultEngagementRequestValidator()
    {
        return new EngagementRequestValidator(
            new List<IEngagementRequestRule>
            {
                new EngagementRequestCodeLengthRule(86400),
                new EngagementRequestCapacityRangeRule(16, 3600),
                new EngagementRequestHoursRangeRule(9.7, 3231.1),
            });
    }
}
