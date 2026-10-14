namespace Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>A rule a scheduled access badge metric must pass. Check returns its problems, or none.</summary>
public interface IScheduledAccessBadgeMetricRule
{
    List<string> Check(ScheduledAccessBadgeMetric item);
}

public sealed record ScheduledAccessBadgeMetricJobCodeLengthRule(int MaxLength) : IScheduledAccessBadgeMetricRule
{
    public List<string> Check(ScheduledAccessBadgeMetric item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccessBadgeMetricRequiredCodeRule(string Message) : IScheduledAccessBadgeMetricRule
{
    public List<string> Check(ScheduledAccessBadgeMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccessBadgeMetricPhoneLengthRule(int MaxLength) : IScheduledAccessBadgeMetricRule
{
    public List<string> Check(ScheduledAccessBadgeMetric item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccessBadgeMetricCodePrefixRule(string Prefix) : IScheduledAccessBadgeMetricRule
{
    public List<string> Check(ScheduledAccessBadgeMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled access badge metric.</summary>
public sealed record ScheduledAccessBadgeMetricValidator(List<IScheduledAccessBadgeMetricRule> Rules)
{
    public List<string> Validate(ScheduledAccessBadgeMetric item)
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

    public void RequireValid(ScheduledAccessBadgeMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAccessBadgeMetric> items)
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

/// <summary>Functions of scheduled access badge metric rules.</summary>
public static class ScheduledAccessBadgeMetricRules
{
    public static ScheduledAccessBadgeMetricValidator DefaultScheduledAccessBadgeMetricValidator()
    {
        return new ScheduledAccessBadgeMetricValidator(
            new List<IScheduledAccessBadgeMetricRule>
            {
                new ScheduledAccessBadgeMetricJobCodeLengthRule(16),
                new ScheduledAccessBadgeMetricRequiredCodeRule("scheduled access badge metric needs a value"),
                new ScheduledAccessBadgeMetricPhoneLengthRule(1000),
                new ScheduledAccessBadgeMetricCodePrefixRule("REF"),
            });
    }
}
