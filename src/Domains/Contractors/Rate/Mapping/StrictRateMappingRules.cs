namespace Acme.Workforce.Contractors.Rate.Mapping;

/// <summary>A rule a strict rate mapping must pass. Check returns its problems, or none.</summary>
public interface IStrictRateMappingRule
{
    List<string> Check(StrictRateMapping item);
}

public sealed record StrictRateMappingThresholdRangeRule(double Min, double Max) : IStrictRateMappingRule
{
    public List<string> Check(StrictRateMapping item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictRateMappingApprovedGuardRule(double Limit) : IStrictRateMappingRule
{
    public List<string> Check(StrictRateMapping item)
    {
        if (item.Approved && item.Threshold > Limit)
        {
            return new List<string> { $"threshold {item.Threshold} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record StrictRateMappingCodePrefixRule(string Prefix) : IStrictRateMappingRule
{
    public List<string> Check(StrictRateMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict rate mapping.</summary>
public sealed record StrictRateMappingValidator(List<IStrictRateMappingRule> Rules)
{
    public List<string> Validate(StrictRateMapping item)
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

    public void RequireValid(StrictRateMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictRateMapping> items)
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

/// <summary>Functions of strict rate mapping rules.</summary>
public static class StrictRateMappingRules
{
    public static StrictRateMappingValidator DefaultStrictRateMappingValidator()
    {
        return new StrictRateMappingValidator(
            new List<IStrictRateMappingRule>
            {
                new StrictRateMappingThresholdRangeRule(3.5, 2596.7),
                new StrictRateMappingApprovedGuardRule(9.7),
                new StrictRateMappingCodePrefixRule("REF"),
            });
    }
}
