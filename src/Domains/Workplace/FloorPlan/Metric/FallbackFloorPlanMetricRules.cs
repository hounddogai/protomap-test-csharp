namespace Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>A rule a fallback floor plan metric must pass. Check returns its problems, or none.</summary>
public interface IFallbackFloorPlanMetricRule
{
    List<string> Check(FallbackFloorPlanMetric item);
}

public sealed record FallbackFloorPlanMetricChecksumLengthRule(int MaxLength) : IFallbackFloorPlanMetricRule
{
    public List<string> Check(FallbackFloorPlanMetric item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackFloorPlanMetricRequiredLabelRule(string Message) : IFallbackFloorPlanMetricRule
{
    public List<string> Check(FallbackFloorPlanMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackFloorPlanMetricLeaveDaysRangeRule(int Min, int Max) : IFallbackFloorPlanMetricRule
{
    public List<string> Check(FallbackFloorPlanMetric item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a fallback floor plan metric.</summary>
public sealed record FallbackFloorPlanMetricValidator(List<IFallbackFloorPlanMetricRule> Rules)
{
    public List<string> Validate(FallbackFloorPlanMetric item)
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

    public void RequireValid(FallbackFloorPlanMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackFloorPlanMetric> items)
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

/// <summary>Functions of fallback floor plan metric rules.</summary>
public static class FallbackFloorPlanMetricRules
{
    public static FallbackFloorPlanMetricValidator DefaultFallbackFloorPlanMetricValidator()
    {
        return new FallbackFloorPlanMetricValidator(
            new List<IFallbackFloorPlanMetricRule>
            {
                new FallbackFloorPlanMetricChecksumLengthRule(3600),
                new FallbackFloorPlanMetricRequiredLabelRule("fallback floor plan metric needs a value"),
                new FallbackFloorPlanMetricLeaveDaysRangeRule(64, 64),
            });
    }
}
