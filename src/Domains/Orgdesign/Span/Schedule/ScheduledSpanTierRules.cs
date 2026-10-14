namespace Acme.Workforce.Orgdesign.Span.Schedule;

/// <summary>A rule a scheduled span tier must pass. Check returns its problems, or none.</summary>
public interface IScheduledSpanTierRule
{
    List<string> Check(ScheduledSpanTier item);
}

public sealed record ScheduledSpanTierPriorityRangeRule(int Min, int Max) : IScheduledSpanTierRule
{
    public List<string> Check(ScheduledSpanTier item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSpanTierAttemptsRangeRule(int Min, int Max) : IScheduledSpanTierRule
{
    public List<string> Check(ScheduledSpanTier item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSpanTierLocationLengthRule(int MaxLength) : IScheduledSpanTierRule
{
    public List<string> Check(ScheduledSpanTier item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSpanTierCodePrefixRule(string Prefix) : IScheduledSpanTierRule
{
    public List<string> Check(ScheduledSpanTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSpanTierRequiredTitleRule(string Message) : IScheduledSpanTierRule
{
    public List<string> Check(ScheduledSpanTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSpanTierTitleLengthRule(int MaxLength) : IScheduledSpanTierRule
{
    public List<string> Check(ScheduledSpanTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled span tier.</summary>
public sealed record ScheduledSpanTierValidator(List<IScheduledSpanTierRule> Rules)
{
    public List<string> Validate(ScheduledSpanTier item)
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

    public void RequireValid(ScheduledSpanTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledSpanTier> items)
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

/// <summary>Functions of scheduled span tier rules.</summary>
public static class ScheduledSpanTierRules
{
    public static ScheduledSpanTierValidator DefaultScheduledSpanTierValidator()
    {
        return new ScheduledSpanTierValidator(
            new List<IScheduledSpanTierRule>
            {
                new ScheduledSpanTierPriorityRangeRule(64, 80),
                new ScheduledSpanTierAttemptsRangeRule(16, 1000),
                new ScheduledSpanTierLocationLengthRule(255),
                new ScheduledSpanTierCodePrefixRule("Q"),
                new ScheduledSpanTierRequiredTitleRule("scheduled span tier needs a value"),
                new ScheduledSpanTierTitleLengthRule(86400),
            });
    }
}
