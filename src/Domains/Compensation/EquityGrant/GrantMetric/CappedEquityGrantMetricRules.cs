namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

/// <summary>A rule a capped equity grant metric must pass. Check returns its problems, or none.</summary>
public interface ICappedEquityGrantMetricRule
{
    List<string> Check(CappedEquityGrantMetric item);
}

public sealed record CappedEquityGrantMetricJobCodeLengthRule(int MaxLength) : ICappedEquityGrantMetricRule
{
    public List<string> Check(CappedEquityGrantMetric item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedEquityGrantMetricRatingRangeRule(double Min, double Max) : ICappedEquityGrantMetricRule
{
    public List<string> Check(CappedEquityGrantMetric item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedEquityGrantMetricApprovedGuardRule(int Limit) : ICappedEquityGrantMetricRule
{
    public List<string> Check(CappedEquityGrantMetric item)
    {
        if (item.Approved && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record CappedEquityGrantMetricCodePrefixRule(string Prefix) : ICappedEquityGrantMetricRule
{
    public List<string> Check(CappedEquityGrantMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedEquityGrantMetricRequiredLabelRule(string Message) : ICappedEquityGrantMetricRule
{
    public List<string> Check(CappedEquityGrantMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedEquityGrantMetricAmountRangeRule(double Min, double Max) : ICappedEquityGrantMetricRule
{
    public List<string> Check(CappedEquityGrantMetric item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped equity grant metric.</summary>
public sealed record CappedEquityGrantMetricValidator(List<ICappedEquityGrantMetricRule> Rules)
{
    public List<string> Validate(CappedEquityGrantMetric item)
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

    public void RequireValid(CappedEquityGrantMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedEquityGrantMetric> items)
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

/// <summary>Functions of capped equity grant metric rules.</summary>
public static class CappedEquityGrantMetricRules
{
    public static CappedEquityGrantMetricValidator DefaultCappedEquityGrantMetricValidator()
    {
        return new CappedEquityGrantMetricValidator(
            new List<ICappedEquityGrantMetricRule>
            {
                new CappedEquityGrantMetricJobCodeLengthRule(120),
                new CappedEquityGrantMetricRatingRangeRule(8.2, 1339.2),
                new CappedEquityGrantMetricApprovedGuardRule(3600),
                new CappedEquityGrantMetricCodePrefixRule("Q"),
                new CappedEquityGrantMetricRequiredLabelRule("capped equity grant metric needs an entry"),
                new CappedEquityGrantMetricAmountRangeRule(4.4, 3038.3),
            });
    }
}
