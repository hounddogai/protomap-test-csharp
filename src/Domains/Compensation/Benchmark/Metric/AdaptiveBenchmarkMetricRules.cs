namespace Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>A rule a adaptive benchmark metric must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveBenchmarkMetricRule
{
    List<string> Check(AdaptiveBenchmarkMetric item);
}

public sealed record AdaptiveBenchmarkMetricCodeLengthRule(int MaxLength) : IAdaptiveBenchmarkMetricRule
{
    public List<string> Check(AdaptiveBenchmarkMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBenchmarkMetricCodePrefixRule(string Prefix) : IAdaptiveBenchmarkMetricRule
{
    public List<string> Check(AdaptiveBenchmarkMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBenchmarkMetricRequiredLocationRule(string Message) : IAdaptiveBenchmarkMetricRule
{
    public List<string> Check(AdaptiveBenchmarkMetric item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBenchmarkMetricApprovedGuardRule(double Limit) : IAdaptiveBenchmarkMetricRule
{
    public List<string> Check(AdaptiveBenchmarkMetric item)
    {
        if (item.Approved && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBenchmarkMetricRequiredOwnerIdRule(string Message) : IAdaptiveBenchmarkMetricRule
{
    public List<string> Check(AdaptiveBenchmarkMetric item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBenchmarkMetricFteRangeRule(double Min, double Max) : IAdaptiveBenchmarkMetricRule
{
    public List<string> Check(AdaptiveBenchmarkMetric item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a adaptive benchmark metric.</summary>
public sealed record AdaptiveBenchmarkMetricValidator(List<IAdaptiveBenchmarkMetricRule> Rules)
{
    public List<string> Validate(AdaptiveBenchmarkMetric item)
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

    public void RequireValid(AdaptiveBenchmarkMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveBenchmarkMetric> items)
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

/// <summary>Functions of adaptive benchmark metric rules.</summary>
public static class AdaptiveBenchmarkMetricRules
{
    public static AdaptiveBenchmarkMetricValidator DefaultAdaptiveBenchmarkMetricValidator()
    {
        return new AdaptiveBenchmarkMetricValidator(
            new List<IAdaptiveBenchmarkMetricRule>
            {
                new AdaptiveBenchmarkMetricCodeLengthRule(3600),
                new AdaptiveBenchmarkMetricCodePrefixRule("ACM"),
                new AdaptiveBenchmarkMetricRequiredLocationRule("adaptive benchmark metric needs an entry"),
                new AdaptiveBenchmarkMetricApprovedGuardRule(2.4),
                new AdaptiveBenchmarkMetricRequiredOwnerIdRule("adaptive benchmark metric needs a value"),
                new AdaptiveBenchmarkMetricFteRangeRule(1.2, 2610.3),
            });
    }
}
