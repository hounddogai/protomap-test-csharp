namespace Acme.Workforce.Contractors.Engagement.Adjustment;

/// <summary>A rule a engagement score must pass. Check returns its problems, or none.</summary>
public interface IEngagementScoreRule
{
    List<string> Check(EngagementScore item);
}

public sealed record EngagementScoreWeightRangeRule(double Min, double Max) : IEngagementScoreRule
{
    public List<string> Check(EngagementScore item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementScoreRequiredBadgeRule(string Message) : IEngagementScoreRule
{
    public List<string> Check(EngagementScore item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementScoreMaximumRangeRule(double Min, double Max) : IEngagementScoreRule
{
    public List<string> Check(EngagementScore item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementScoreLocationLengthRule(int MaxLength) : IEngagementScoreRule
{
    public List<string> Check(EngagementScore item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement score.</summary>
public sealed record EngagementScoreValidator(List<IEngagementScoreRule> Rules)
{
    public List<string> Validate(EngagementScore item)
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

    public void RequireValid(EngagementScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementScore> items)
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

/// <summary>Functions of engagement score rules.</summary>
public static class EngagementScoreRules
{
    public static EngagementScoreValidator DefaultEngagementScoreValidator()
    {
        return new EngagementScoreValidator(
            new List<IEngagementScoreRule>
            {
                new EngagementScoreWeightRangeRule(7.7, 2741.2),
                new EngagementScoreRequiredBadgeRule("engagement score needs an entry"),
                new EngagementScoreMaximumRangeRule(4.4, 2974.5),
                new EngagementScoreLocationLengthRule(120),
            });
    }
}
