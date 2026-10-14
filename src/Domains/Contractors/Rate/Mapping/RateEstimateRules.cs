namespace Acme.Workforce.Contractors.Rate.Mapping;

/// <summary>A rule a rate estimate must pass. Check returns its problems, or none.</summary>
public interface IRateEstimateRule
{
    List<string> Check(RateEstimate item);
}

public sealed record RateEstimateExternalRefLengthRule(int MaxLength) : IRateEstimateRule
{
    public List<string> Check(RateEstimate item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateEstimateCodePrefixRule(string Prefix) : IRateEstimateRule
{
    public List<string> Check(RateEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RateEstimateThresholdRangeRule(double Min, double Max) : IRateEstimateRule
{
    public List<string> Check(RateEstimate item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RateEstimateRequiredTitleRule(string Message) : IRateEstimateRule
{
    public List<string> Check(RateEstimate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateEstimateTitleLengthRule(int MaxLength) : IRateEstimateRule
{
    public List<string> Check(RateEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateEstimatePriorityRangeRule(int Min, int Max) : IRateEstimateRule
{
    public List<string> Check(RateEstimate item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate estimate.</summary>
public sealed record RateEstimateValidator(List<IRateEstimateRule> Rules)
{
    public List<string> Validate(RateEstimate item)
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

    public void RequireValid(RateEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateEstimate> items)
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

/// <summary>Functions of rate estimate rules.</summary>
public static class RateEstimateRules
{
    public static RateEstimateValidator DefaultRateEstimateValidator()
    {
        return new RateEstimateValidator(
            new List<IRateEstimateRule>
            {
                new RateEstimateExternalRefLengthRule(3600),
                new RateEstimateCodePrefixRule("Q"),
                new RateEstimateThresholdRangeRule(0.5, 2560.4),
                new RateEstimateRequiredTitleRule("rate estimate needs this field"),
                new RateEstimateTitleLengthRule(120),
                new RateEstimatePriorityRangeRule(32, 120),
            });
    }
}
