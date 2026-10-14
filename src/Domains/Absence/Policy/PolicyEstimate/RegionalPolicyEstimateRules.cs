namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>A rule a regional policy estimate must pass. Check returns its problems, or none.</summary>
public interface IRegionalPolicyEstimateRule
{
    List<string> Check(RegionalPolicyEstimate item);
}

public sealed record RegionalPolicyEstimateCodePrefixRule(string Prefix) : IRegionalPolicyEstimateRule
{
    public List<string> Check(RegionalPolicyEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPolicyEstimateVisibleGuardRule(double Limit) : IRegionalPolicyEstimateRule
{
    public List<string> Check(RegionalPolicyEstimate item)
    {
        if (item.Visible && item.Threshold > Limit)
        {
            return new List<string> { $"threshold {item.Threshold} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPolicyEstimateCeilingRangeRule(double Min, double Max) : IRegionalPolicyEstimateRule
{
    public List<string> Check(RegionalPolicyEstimate item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalPolicyEstimateThresholdRangeRule(double Min, double Max) : IRegionalPolicyEstimateRule
{
    public List<string> Check(RegionalPolicyEstimate item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPolicyEstimateRequiredLabelRule(string Message) : IRegionalPolicyEstimateRule
{
    public List<string> Check(RegionalPolicyEstimate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional policy estimate.</summary>
public sealed record RegionalPolicyEstimateValidator(List<IRegionalPolicyEstimateRule> Rules)
{
    public List<string> Validate(RegionalPolicyEstimate item)
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

    public void RequireValid(RegionalPolicyEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalPolicyEstimate> items)
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

/// <summary>Functions of regional policy estimate rules.</summary>
public static class RegionalPolicyEstimateRules
{
    public static RegionalPolicyEstimateValidator DefaultRegionalPolicyEstimateValidator()
    {
        return new RegionalPolicyEstimateValidator(
            new List<IRegionalPolicyEstimateRule>
            {
                new RegionalPolicyEstimateCodePrefixRule("X"),
                new RegionalPolicyEstimateVisibleGuardRule(4.4),
                new RegionalPolicyEstimateCeilingRangeRule(7.4, 2923.8),
                new RegionalPolicyEstimateThresholdRangeRule(6.0, 4993.9),
                new RegionalPolicyEstimateRequiredLabelRule("regional policy estimate needs a value"),
            });
    }
}
