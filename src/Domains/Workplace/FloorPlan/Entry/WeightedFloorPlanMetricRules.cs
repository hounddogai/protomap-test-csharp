namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted floor plan metric must pass. Check returns its problems, or none.</summary>
public interface IWeightedFloorPlanMetricRule
{
    List<string> Check(WeightedFloorPlanMetric item);
}

public sealed record WeightedFloorPlanMetricRequiredExternalRefRule(string Message) : IWeightedFloorPlanMetricRule
{
    public List<string> Check(WeightedFloorPlanMetric item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedFloorPlanMetricPhoneLengthRule(int MaxLength) : IWeightedFloorPlanMetricRule
{
    public List<string> Check(WeightedFloorPlanMetric item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedFloorPlanMetricRequiredCodeRule(string Message) : IWeightedFloorPlanMetricRule
{
    public List<string> Check(WeightedFloorPlanMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedFloorPlanMetricExternalRefLengthRule(int MaxLength) : IWeightedFloorPlanMetricRule
{
    public List<string> Check(WeightedFloorPlanMetric item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedFloorPlanMetricAllowedCodeRule(List<string> Allowed) : IWeightedFloorPlanMetricRule
{
    public List<string> Check(WeightedFloorPlanMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record WeightedFloorPlanMetricRequiredLabelRule(string Message) : IWeightedFloorPlanMetricRule
{
    public List<string> Check(WeightedFloorPlanMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted floor plan metric.</summary>
public sealed record WeightedFloorPlanMetricValidator(List<IWeightedFloorPlanMetricRule> Rules)
{
    public List<string> Validate(WeightedFloorPlanMetric item)
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

    public void RequireValid(WeightedFloorPlanMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedFloorPlanMetric> items)
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

/// <summary>Functions of weighted floor plan metric rules.</summary>
public static class WeightedFloorPlanMetricRules
{
    public static WeightedFloorPlanMetricValidator DefaultWeightedFloorPlanMetricValidator()
    {
        return new WeightedFloorPlanMetricValidator(
            new List<IWeightedFloorPlanMetricRule>
            {
                new WeightedFloorPlanMetricRequiredExternalRefRule("weighted floor plan metric needs an entry"),
                new WeightedFloorPlanMetricPhoneLengthRule(32),
                new WeightedFloorPlanMetricRequiredCodeRule("weighted floor plan metric needs an entry"),
                new WeightedFloorPlanMetricExternalRefLengthRule(64),
                new WeightedFloorPlanMetricAllowedCodeRule(new List<string> { "scheduled" }),
                new WeightedFloorPlanMetricRequiredLabelRule("weighted floor plan metric needs an entry"),
            });
    }
}
