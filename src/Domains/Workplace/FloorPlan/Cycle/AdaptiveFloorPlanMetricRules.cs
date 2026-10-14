namespace Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>A rule a adaptive floor plan metric must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveFloorPlanMetricRule
{
    List<string> Check(AdaptiveFloorPlanMetric item);
}

public sealed record AdaptiveFloorPlanMetricCodePrefixRule(string Prefix) : IAdaptiveFloorPlanMetricRule
{
    public List<string> Check(AdaptiveFloorPlanMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFloorPlanMetricLabelLengthRule(int MaxLength) : IAdaptiveFloorPlanMetricRule
{
    public List<string> Check(AdaptiveFloorPlanMetric item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFloorPlanMetricRatioRangeRule(double Min, double Max) : IAdaptiveFloorPlanMetricRule
{
    public List<string> Check(AdaptiveFloorPlanMetric item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFloorPlanMetricCodeLengthRule(int MaxLength) : IAdaptiveFloorPlanMetricRule
{
    public List<string> Check(AdaptiveFloorPlanMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive floor plan metric.</summary>
public sealed record AdaptiveFloorPlanMetricValidator(List<IAdaptiveFloorPlanMetricRule> Rules)
{
    public List<string> Validate(AdaptiveFloorPlanMetric item)
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

    public void RequireValid(AdaptiveFloorPlanMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveFloorPlanMetric> items)
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

/// <summary>Functions of adaptive floor plan metric rules.</summary>
public static class AdaptiveFloorPlanMetricRules
{
    public static AdaptiveFloorPlanMetricValidator DefaultAdaptiveFloorPlanMetricValidator()
    {
        return new AdaptiveFloorPlanMetricValidator(
            new List<IAdaptiveFloorPlanMetricRule>
            {
                new AdaptiveFloorPlanMetricCodePrefixRule("Q"),
                new AdaptiveFloorPlanMetricLabelLengthRule(32),
                new AdaptiveFloorPlanMetricRatioRangeRule(5.2, 3996.1),
                new AdaptiveFloorPlanMetricCodeLengthRule(1000),
            });
    }
}
