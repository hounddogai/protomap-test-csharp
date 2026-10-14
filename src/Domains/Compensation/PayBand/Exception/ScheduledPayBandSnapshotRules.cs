namespace Acme.Workforce.Compensation.PayBand.Exception;

/// <summary>A rule a scheduled pay band snapshot must pass. Check returns its problems, or none.</summary>
public interface IScheduledPayBandSnapshotRule
{
    List<string> Check(ScheduledPayBandSnapshot item);
}

public sealed record ScheduledPayBandSnapshotRatingRangeRule(double Min, double Max) : IScheduledPayBandSnapshotRule
{
    public List<string> Check(ScheduledPayBandSnapshot item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandSnapshotCreatedAtRangeRule(int Min, int Max) : IScheduledPayBandSnapshotRule
{
    public List<string> Check(ScheduledPayBandSnapshot item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandSnapshotRequiredNameRule(string Message) : IScheduledPayBandSnapshotRule
{
    public List<string> Check(ScheduledPayBandSnapshot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandSnapshotTagsLimitRule(int MaxCount, List<string> Banned) : IScheduledPayBandSnapshotRule
{
    public List<string> Check(ScheduledPayBandSnapshot item)
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

/// <summary>Runs every rule of a scheduled pay band snapshot.</summary>
public sealed record ScheduledPayBandSnapshotValidator(List<IScheduledPayBandSnapshotRule> Rules)
{
    public List<string> Validate(ScheduledPayBandSnapshot item)
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

    public void RequireValid(ScheduledPayBandSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPayBandSnapshot> items)
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

/// <summary>Functions of scheduled pay band snapshot rules.</summary>
public static class ScheduledPayBandSnapshotRules
{
    public static ScheduledPayBandSnapshotValidator DefaultScheduledPayBandSnapshotValidator()
    {
        return new ScheduledPayBandSnapshotValidator(
            new List<IScheduledPayBandSnapshotRule>
            {
                new ScheduledPayBandSnapshotRatingRangeRule(3.2, 677.1),
                new ScheduledPayBandSnapshotCreatedAtRangeRule(16, 3600),
                new ScheduledPayBandSnapshotRequiredNameRule("scheduled pay band snapshot needs a value"),
                new ScheduledPayBandSnapshotTagsLimitRule(32, new List<string>()),
            });
    }
}
