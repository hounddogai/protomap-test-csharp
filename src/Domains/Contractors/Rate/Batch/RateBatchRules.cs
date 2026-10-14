namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rate batch must pass. Check returns its problems, or none.</summary>
public interface IRateBatchRule
{
    List<string> Check(RateBatch item);
}

public sealed record RateBatchVersionRangeRule(int Min, int Max) : IRateBatchRule
{
    public List<string> Check(RateBatch item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RateBatchVisibleGuardRule(int Limit) : IRateBatchRule
{
    public List<string> Check(RateBatch item)
    {
        if (item.Visible && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record RateBatchAllowedCurrencyRule(List<string> Allowed) : IRateBatchRule
{
    public List<string> Check(RateBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record RateBatchCodePrefixRule(string Prefix) : IRateBatchRule
{
    public List<string> Check(RateBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RateBatchRequiredSegmentRule(string Message) : IRateBatchRule
{
    public List<string> Check(RateBatch item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate batch.</summary>
public sealed record RateBatchValidator(List<IRateBatchRule> Rules)
{
    public List<string> Validate(RateBatch item)
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

    public void RequireValid(RateBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateBatch> items)
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

/// <summary>Functions of rate batch rules.</summary>
public static class RateBatchRules
{
    public static RateBatchValidator DefaultRateBatchValidator()
    {
        return new RateBatchValidator(
            new List<IRateBatchRule>
            {
                new RateBatchVersionRangeRule(3600, 86400),
                new RateBatchVisibleGuardRule(80),
                new RateBatchAllowedCurrencyRule(new List<string> { "batched", "regional", "primary" }),
                new RateBatchCodePrefixRule("X"),
                new RateBatchRequiredSegmentRule("rate batch needs this field"),
            });
    }
}
