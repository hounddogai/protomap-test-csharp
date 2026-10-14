namespace Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>A rule a scheduled span digest must pass. Check returns its problems, or none.</summary>
public interface IScheduledSpanDigestRule
{
    List<string> Check(ScheduledSpanDigest item);
}

public sealed record ScheduledSpanDigestRequiredLabelRule(string Message) : IScheduledSpanDigestRule
{
    public List<string> Check(ScheduledSpanDigest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSpanDigestStartsAtRangeRule(int Min, int Max) : IScheduledSpanDigestRule
{
    public List<string> Check(ScheduledSpanDigest item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledSpanDigestRequiredCodeRule(string Message) : IScheduledSpanDigestRule
{
    public List<string> Check(ScheduledSpanDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSpanDigestExternalRefLengthRule(int MaxLength) : IScheduledSpanDigestRule
{
    public List<string> Check(ScheduledSpanDigest item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSpanDigestEndsAtRangeRule(int Min, int Max) : IScheduledSpanDigestRule
{
    public List<string> Check(ScheduledSpanDigest item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSpanDigestRatioRangeRule(double Min, double Max) : IScheduledSpanDigestRule
{
    public List<string> Check(ScheduledSpanDigest item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled span digest.</summary>
public sealed record ScheduledSpanDigestValidator(List<IScheduledSpanDigestRule> Rules)
{
    public List<string> Validate(ScheduledSpanDigest item)
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

    public void RequireValid(ScheduledSpanDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledSpanDigest> items)
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

/// <summary>Functions of scheduled span digest rules.</summary>
public static class ScheduledSpanDigestRules
{
    public static ScheduledSpanDigestValidator DefaultScheduledSpanDigestValidator()
    {
        return new ScheduledSpanDigestValidator(
            new List<IScheduledSpanDigestRule>
            {
                new ScheduledSpanDigestRequiredLabelRule("scheduled span digest needs an entry"),
                new ScheduledSpanDigestStartsAtRangeRule(80, 120),
                new ScheduledSpanDigestRequiredCodeRule("scheduled span digest needs an entry"),
                new ScheduledSpanDigestExternalRefLengthRule(255),
                new ScheduledSpanDigestEndsAtRangeRule(16, 120),
                new ScheduledSpanDigestRatioRangeRule(2.1, 3793.8),
            });
    }
}
