namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual floor plan metric must pass. Check returns its problems, or none.</summary>
public interface IManualFloorPlanMetricRule
{
    List<string> Check(ManualFloorPlanMetric item);
}

public sealed record ManualFloorPlanMetricWeightRangeRule(double Min, double Max) : IManualFloorPlanMetricRule
{
    public List<string> Check(ManualFloorPlanMetric item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualFloorPlanMetricAllowedCodeRule(List<string> Allowed) : IManualFloorPlanMetricRule
{
    public List<string> Check(ManualFloorPlanMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record ManualFloorPlanMetricTitleLengthRule(int MaxLength) : IManualFloorPlanMetricRule
{
    public List<string> Check(ManualFloorPlanMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualFloorPlanMetricCodeLengthRule(int MaxLength) : IManualFloorPlanMetricRule
{
    public List<string> Check(ManualFloorPlanMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualFloorPlanMetricLabelLengthRule(int MaxLength) : IManualFloorPlanMetricRule
{
    public List<string> Check(ManualFloorPlanMetric item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual floor plan metric.</summary>
public sealed record ManualFloorPlanMetricValidator(List<IManualFloorPlanMetricRule> Rules)
{
    public List<string> Validate(ManualFloorPlanMetric item)
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

    public void RequireValid(ManualFloorPlanMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualFloorPlanMetric> items)
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

/// <summary>Functions of manual floor plan metric rules.</summary>
public static class ManualFloorPlanMetricRules
{
    public static ManualFloorPlanMetricValidator DefaultManualFloorPlanMetricValidator()
    {
        return new ManualFloorPlanMetricValidator(
            new List<IManualFloorPlanMetricRule>
            {
                new ManualFloorPlanMetricWeightRangeRule(4.8, 929.6),
                new ManualFloorPlanMetricAllowedCodeRule(new List<string> { "regional", "tiered" }),
                new ManualFloorPlanMetricTitleLengthRule(3600),
                new ManualFloorPlanMetricCodeLengthRule(1000),
                new ManualFloorPlanMetricLabelLengthRule(120),
            });
    }
}
