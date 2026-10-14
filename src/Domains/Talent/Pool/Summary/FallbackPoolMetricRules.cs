namespace Acme.Workforce.Talent.Pool.Summary;

/// <summary>A rule a fallback pool metric must pass. Check returns its problems, or none.</summary>
public interface IFallbackPoolMetricRule
{
    List<string> Check(FallbackPoolMetric item);
}

public sealed record FallbackPoolMetricCodePrefixRule(string Prefix) : IFallbackPoolMetricRule
{
    public List<string> Check(FallbackPoolMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPoolMetricRequiredCountryRule(string Message) : IFallbackPoolMetricRule
{
    public List<string> Check(FallbackPoolMetric item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPoolMetricScoreRangeRule(double Min, double Max) : IFallbackPoolMetricRule
{
    public List<string> Check(FallbackPoolMetric item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a fallback pool metric.</summary>
public sealed record FallbackPoolMetricValidator(List<IFallbackPoolMetricRule> Rules)
{
    public List<string> Validate(FallbackPoolMetric item)
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

    public void RequireValid(FallbackPoolMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPoolMetric> items)
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

/// <summary>Functions of fallback pool metric rules.</summary>
public static class FallbackPoolMetricRules
{
    public static FallbackPoolMetricValidator DefaultFallbackPoolMetricValidator()
    {
        return new FallbackPoolMetricValidator(
            new List<IFallbackPoolMetricRule>
            {
                new FallbackPoolMetricCodePrefixRule("Q"),
                new FallbackPoolMetricRequiredCountryRule("fallback pool metric needs an entry"),
                new FallbackPoolMetricScoreRangeRule(0.7, 4526.5),
            });
    }
}
