namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

/// <summary>A rule a regional engagement override must pass. Check returns its problems, or none.</summary>
public interface IRegionalEngagementOverrideRule
{
    List<string> Check(RegionalEngagementOverride item);
}

public sealed record RegionalEngagementOverrideRequiredSegmentRule(string Message) : IRegionalEngagementOverrideRule
{
    public List<string> Check(RegionalEngagementOverride item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalEngagementOverrideRequiredLocationRule(string Message) : IRegionalEngagementOverrideRule
{
    public List<string> Check(RegionalEngagementOverride item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalEngagementOverrideLocationLengthRule(int MaxLength) : IRegionalEngagementOverrideRule
{
    public List<string> Check(RegionalEngagementOverride item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalEngagementOverrideUpdatedAtRangeRule(int Min, int Max) : IRegionalEngagementOverrideRule
{
    public List<string> Check(RegionalEngagementOverride item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional engagement override.</summary>
public sealed record RegionalEngagementOverrideValidator(List<IRegionalEngagementOverrideRule> Rules)
{
    public List<string> Validate(RegionalEngagementOverride item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(RegionalEngagementOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalEngagementOverride> items)
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

/// <summary>Functions of regional engagement override rules.</summary>
public static class RegionalEngagementOverrideRules
{
    public static RegionalEngagementOverrideValidator DefaultRegionalEngagementOverrideValidator()
    {
        return new RegionalEngagementOverrideValidator(
            new List<IRegionalEngagementOverrideRule>
            {
                new RegionalEngagementOverrideRequiredSegmentRule("regional engagement override needs a value"),
                new RegionalEngagementOverrideRequiredLocationRule("regional engagement override needs an entry"),
                new RegionalEngagementOverrideLocationLengthRule(1000),
                new RegionalEngagementOverrideUpdatedAtRangeRule(16, 32),
            });
    }
}
