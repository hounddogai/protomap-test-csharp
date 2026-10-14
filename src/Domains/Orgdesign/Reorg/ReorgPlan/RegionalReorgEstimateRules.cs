namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>A rule a regional reorg estimate must pass. Check returns its problems, or none.</summary>
public interface IRegionalReorgEstimateRule
{
    List<string> Check(RegionalReorgEstimate item);
}

public sealed record RegionalReorgEstimateRequiredSegmentRule(string Message) : IRegionalReorgEstimateRule
{
    public List<string> Check(RegionalReorgEstimate item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalReorgEstimateEndsAtRangeRule(int Min, int Max) : IRegionalReorgEstimateRule
{
    public List<string> Check(RegionalReorgEstimate item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalReorgEstimateRatioRangeRule(double Min, double Max) : IRegionalReorgEstimateRule
{
    public List<string> Check(RegionalReorgEstimate item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalReorgEstimateRequiredCodeRule(string Message) : IRegionalReorgEstimateRule
{
    public List<string> Check(RegionalReorgEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalReorgEstimateLabelLengthRule(int MaxLength) : IRegionalReorgEstimateRule
{
    public List<string> Check(RegionalReorgEstimate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalReorgEstimateCodePrefixRule(string Prefix) : IRegionalReorgEstimateRule
{
    public List<string> Check(RegionalReorgEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional reorg estimate.</summary>
public sealed record RegionalReorgEstimateValidator(List<IRegionalReorgEstimateRule> Rules)
{
    public List<string> Validate(RegionalReorgEstimate item)
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

    public void RequireValid(RegionalReorgEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalReorgEstimate> items)
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

/// <summary>Functions of regional reorg estimate rules.</summary>
public static class RegionalReorgEstimateRules
{
    public static RegionalReorgEstimateValidator DefaultRegionalReorgEstimateValidator()
    {
        return new RegionalReorgEstimateValidator(
            new List<IRegionalReorgEstimateRule>
            {
                new RegionalReorgEstimateRequiredSegmentRule("regional reorg estimate needs a value"),
                new RegionalReorgEstimateEndsAtRangeRule(1000, 3600),
                new RegionalReorgEstimateRatioRangeRule(5.3, 3916.3),
                new RegionalReorgEstimateRequiredCodeRule("regional reorg estimate needs a value"),
                new RegionalReorgEstimateLabelLengthRule(86400),
                new RegionalReorgEstimateCodePrefixRule("REF"),
            });
    }
}
