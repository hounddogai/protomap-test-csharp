namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

/// <summary>A rule a adjustment estimate must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentEstimateRule
{
    List<string> Check(AdjustmentEstimate item);
}

public sealed record AdjustmentEstimateCodePrefixRule(string Prefix) : IAdjustmentEstimateRule
{
    public List<string> Check(AdjustmentEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentEstimateRequiredLocaleRule(string Message) : IAdjustmentEstimateRule
{
    public List<string> Check(AdjustmentEstimate item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentEstimateMarginRangeRule(double Min, double Max) : IAdjustmentEstimateRule
{
    public List<string> Check(AdjustmentEstimate item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentEstimateRequiredTitleRule(string Message) : IAdjustmentEstimateRule
{
    public List<string> Check(AdjustmentEstimate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment estimate.</summary>
public sealed record AdjustmentEstimateValidator(List<IAdjustmentEstimateRule> Rules)
{
    public List<string> Validate(AdjustmentEstimate item)
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

    public void RequireValid(AdjustmentEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentEstimate> items)
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

/// <summary>Functions of adjustment estimate rules.</summary>
public static class AdjustmentEstimateRules
{
    public static AdjustmentEstimateValidator DefaultAdjustmentEstimateValidator()
    {
        return new AdjustmentEstimateValidator(
            new List<IAdjustmentEstimateRule>
            {
                new AdjustmentEstimateCodePrefixRule("OPS"),
                new AdjustmentEstimateRequiredLocaleRule("adjustment estimate needs a value"),
                new AdjustmentEstimateMarginRangeRule(0.8, 1619.5),
                new AdjustmentEstimateRequiredTitleRule("adjustment estimate needs an entry"),
            });
    }
}
