namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>A rule a legacy access badge metric must pass. Check returns its problems, or none.</summary>
public interface ILegacyAccessBadgeMetricRule
{
    List<string> Check(LegacyAccessBadgeMetric item);
}

public sealed record LegacyAccessBadgeMetricRequiredLabelRule(string Message) : ILegacyAccessBadgeMetricRule
{
    public List<string> Check(LegacyAccessBadgeMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAccessBadgeMetricRequiredLocaleRule(string Message) : ILegacyAccessBadgeMetricRule
{
    public List<string> Check(LegacyAccessBadgeMetric item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAccessBadgeMetricTenureMonthsRangeRule(int Min, int Max) : ILegacyAccessBadgeMetricRule
{
    public List<string> Check(LegacyAccessBadgeMetric item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAccessBadgeMetricBatchSizeRangeRule(int Min, int Max) : ILegacyAccessBadgeMetricRule
{
    public List<string> Check(LegacyAccessBadgeMetric item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAccessBadgeMetricRequiredNameRule(string Message) : ILegacyAccessBadgeMetricRule
{
    public List<string> Check(LegacyAccessBadgeMetric item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAccessBadgeMetricLabelLengthRule(int MaxLength) : ILegacyAccessBadgeMetricRule
{
    public List<string> Check(LegacyAccessBadgeMetric item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy access badge metric.</summary>
public sealed record LegacyAccessBadgeMetricValidator(List<ILegacyAccessBadgeMetricRule> Rules)
{
    public List<string> Validate(LegacyAccessBadgeMetric item)
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

    public void RequireValid(LegacyAccessBadgeMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyAccessBadgeMetric> items)
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

/// <summary>Functions of legacy access badge metric rules.</summary>
public static class LegacyAccessBadgeMetricRules
{
    public static LegacyAccessBadgeMetricValidator DefaultLegacyAccessBadgeMetricValidator()
    {
        return new LegacyAccessBadgeMetricValidator(
            new List<ILegacyAccessBadgeMetricRule>
            {
                new LegacyAccessBadgeMetricRequiredLabelRule("legacy access badge metric needs an entry"),
                new LegacyAccessBadgeMetricRequiredLocaleRule("legacy access badge metric needs an entry"),
                new LegacyAccessBadgeMetricTenureMonthsRangeRule(32, 64),
                new LegacyAccessBadgeMetricBatchSizeRangeRule(120, 86400),
                new LegacyAccessBadgeMetricRequiredNameRule("legacy access badge metric needs an entry"),
                new LegacyAccessBadgeMetricLabelLengthRule(86400),
            });
    }
}
