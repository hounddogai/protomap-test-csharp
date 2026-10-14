namespace Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>A rule a regional pool metric must pass. Check returns its problems, or none.</summary>
public interface IRegionalPoolMetricRule
{
    List<string> Check(RegionalPoolMetric item);
}

public sealed record RegionalPoolMetricRetriesRangeRule(int Min, int Max) : IRegionalPoolMetricRule
{
    public List<string> Check(RegionalPoolMetric item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalPoolMetricRequiredOwnerIdRule(string Message) : IRegionalPoolMetricRule
{
    public List<string> Check(RegionalPoolMetric item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalPoolMetricThresholdRangeRule(double Min, double Max) : IRegionalPoolMetricRule
{
    public List<string> Check(RegionalPoolMetric item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPoolMetricCodePrefixRule(string Prefix) : IRegionalPoolMetricRule
{
    public List<string> Check(RegionalPoolMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional pool metric.</summary>
public sealed record RegionalPoolMetricValidator(List<IRegionalPoolMetricRule> Rules)
{
    public List<string> Validate(RegionalPoolMetric item)
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

    public void RequireValid(RegionalPoolMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalPoolMetric> items)
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

/// <summary>Functions of regional pool metric rules.</summary>
public static class RegionalPoolMetricRules
{
    public static RegionalPoolMetricValidator DefaultRegionalPoolMetricValidator()
    {
        return new RegionalPoolMetricValidator(
            new List<IRegionalPoolMetricRule>
            {
                new RegionalPoolMetricRetriesRangeRule(64, 80),
                new RegionalPoolMetricRequiredOwnerIdRule("regional pool metric needs an entry"),
                new RegionalPoolMetricThresholdRangeRule(0.8, 3976.9),
                new RegionalPoolMetricCodePrefixRule("OPS"),
            });
    }
}
