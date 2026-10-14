namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

/// <summary>A rule a engagement override must pass. Check returns its problems, or none.</summary>
public interface IEngagementOverrideRule
{
    List<string> Check(EngagementOverride item);
}

public sealed record EngagementOverrideCapacityRangeRule(int Min, int Max) : IEngagementOverrideRule
{
    public List<string> Check(EngagementOverride item)
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

public sealed record EngagementOverrideLocationLengthRule(int MaxLength) : IEngagementOverrideRule
{
    public List<string> Check(EngagementOverride item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementOverrideNameLengthRule(int MaxLength) : IEngagementOverrideRule
{
    public List<string> Check(EngagementOverride item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementOverrideSourceLengthRule(int MaxLength) : IEngagementOverrideRule
{
    public List<string> Check(EngagementOverride item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementOverrideRequiredCodeRule(string Message) : IEngagementOverrideRule
{
    public List<string> Check(EngagementOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement override.</summary>
public sealed record EngagementOverrideValidator(List<IEngagementOverrideRule> Rules)
{
    public List<string> Validate(EngagementOverride item)
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

    public void RequireValid(EngagementOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementOverride> items)
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

/// <summary>Functions of engagement override rules.</summary>
public static class EngagementOverrideRules
{
    public static EngagementOverrideValidator DefaultEngagementOverrideValidator()
    {
        return new EngagementOverrideValidator(
            new List<IEngagementOverrideRule>
            {
                new EngagementOverrideCapacityRangeRule(32, 3600),
                new EngagementOverrideLocationLengthRule(80),
                new EngagementOverrideNameLengthRule(3600),
                new EngagementOverrideSourceLengthRule(86400),
                new EngagementOverrideRequiredCodeRule("engagement override needs this field"),
            });
    }
}
