namespace Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>A rule a adjustment baseline must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentBaselineRule
{
    List<string> Check(AdjustmentBaseline item);
}

public sealed record AdjustmentBaselineGraceDaysRangeRule(int Min, int Max) : IAdjustmentBaselineRule
{
    public List<string> Check(AdjustmentBaseline item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdjustmentBaselineThresholdRangeRule(double Min, double Max) : IAdjustmentBaselineRule
{
    public List<string> Check(AdjustmentBaseline item)
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

public sealed record AdjustmentBaselineRequiredCodeRule(string Message) : IAdjustmentBaselineRule
{
    public List<string> Check(AdjustmentBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentBaselineCodePrefixRule(string Prefix) : IAdjustmentBaselineRule
{
    public List<string> Check(AdjustmentBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment baseline.</summary>
public sealed record AdjustmentBaselineValidator(List<IAdjustmentBaselineRule> Rules)
{
    public List<string> Validate(AdjustmentBaseline item)
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

    public void RequireValid(AdjustmentBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentBaseline> items)
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

/// <summary>Functions of adjustment baseline rules.</summary>
public static class AdjustmentBaselineRules
{
    public static AdjustmentBaselineValidator DefaultAdjustmentBaselineValidator()
    {
        return new AdjustmentBaselineValidator(
            new List<IAdjustmentBaselineRule>
            {
                new AdjustmentBaselineGraceDaysRangeRule(16, 64),
                new AdjustmentBaselineThresholdRangeRule(8.5, 4398.9),
                new AdjustmentBaselineRequiredCodeRule("adjustment baseline needs this field"),
                new AdjustmentBaselineCodePrefixRule("Q"),
            });
    }
}
