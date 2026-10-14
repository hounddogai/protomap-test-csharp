namespace Acme.Workforce.Talent.Pool.Exception;

/// <summary>A rule a scheduled pool threshold must pass. Check returns its problems, or none.</summary>
public interface IScheduledPoolThresholdRule
{
    List<string> Check(ScheduledPoolThreshold item);
}

public sealed record ScheduledPoolThresholdRevisionRangeRule(int Min, int Max) : IScheduledPoolThresholdRule
{
    public List<string> Check(ScheduledPoolThreshold item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPoolThresholdTagsLimitRule(int MaxCount, List<string> Banned) : IScheduledPoolThresholdRule
{
    public List<string> Check(ScheduledPoolThreshold item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record ScheduledPoolThresholdCodePrefixRule(string Prefix) : IScheduledPoolThresholdRule
{
    public List<string> Check(ScheduledPoolThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pool threshold.</summary>
public sealed record ScheduledPoolThresholdValidator(List<IScheduledPoolThresholdRule> Rules)
{
    public List<string> Validate(ScheduledPoolThreshold item)
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

    public void RequireValid(ScheduledPoolThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPoolThreshold> items)
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

/// <summary>Functions of scheduled pool threshold rules.</summary>
public static class ScheduledPoolThresholdRules
{
    public static ScheduledPoolThresholdValidator DefaultScheduledPoolThresholdValidator()
    {
        return new ScheduledPoolThresholdValidator(
            new List<IScheduledPoolThresholdRule>
            {
                new ScheduledPoolThresholdRevisionRangeRule(16, 32),
                new ScheduledPoolThresholdTagsLimitRule(1000, new List<string> { "rolling" }),
                new ScheduledPoolThresholdCodePrefixRule("REF"),
            });
    }
}
