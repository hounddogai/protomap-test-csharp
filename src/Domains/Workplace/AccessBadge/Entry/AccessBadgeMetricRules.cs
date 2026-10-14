namespace Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>A rule a access badge metric must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeMetricRule
{
    List<string> Check(AccessBadgeMetric item);
}

public sealed record AccessBadgeMetricCodePrefixRule(string Prefix) : IAccessBadgeMetricRule
{
    public List<string> Check(AccessBadgeMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeMetricRequiredLabelRule(string Message) : IAccessBadgeMetricRule
{
    public List<string> Check(AccessBadgeMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeMetricRequiredCodeRule(string Message) : IAccessBadgeMetricRule
{
    public List<string> Check(AccessBadgeMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeMetricRevisionRangeRule(int Min, int Max) : IAccessBadgeMetricRule
{
    public List<string> Check(AccessBadgeMetric item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge metric.</summary>
public sealed record AccessBadgeMetricValidator(List<IAccessBadgeMetricRule> Rules)
{
    public List<string> Validate(AccessBadgeMetric item)
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

    public void RequireValid(AccessBadgeMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeMetric> items)
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

/// <summary>Functions of access badge metric rules.</summary>
public static class AccessBadgeMetricRules
{
    public static AccessBadgeMetricValidator DefaultAccessBadgeMetricValidator()
    {
        return new AccessBadgeMetricValidator(
            new List<IAccessBadgeMetricRule>
            {
                new AccessBadgeMetricCodePrefixRule("OPS"),
                new AccessBadgeMetricRequiredLabelRule("access badge metric needs an entry"),
                new AccessBadgeMetricRequiredCodeRule("access badge metric needs a value"),
                new AccessBadgeMetricRevisionRangeRule(32, 255),
            });
    }
}
