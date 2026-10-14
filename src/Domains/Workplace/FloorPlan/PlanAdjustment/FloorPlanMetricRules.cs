namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>A rule a floor plan metric must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanMetricRule
{
    List<string> Check(FloorPlanMetric item);
}

public sealed record FloorPlanMetricRequiredNameRule(string Message) : IFloorPlanMetricRule
{
    public List<string> Check(FloorPlanMetric item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanMetricRequiredCodeRule(string Message) : IFloorPlanMetricRule
{
    public List<string> Check(FloorPlanMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanMetricRequiredTitleRule(string Message) : IFloorPlanMetricRule
{
    public List<string> Check(FloorPlanMetric item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanMetricVisibleGuardRule(double Limit) : IFloorPlanMetricRule
{
    public List<string> Check(FloorPlanMetric item)
    {
        if (item.Visible && item.Ceiling > Limit)
        {
            return new List<string> { $"ceiling {item.Ceiling} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanMetricCeilingRangeRule(double Min, double Max) : IFloorPlanMetricRule
{
    public List<string> Check(FloorPlanMetric item)
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

public sealed record FloorPlanMetricCodePrefixRule(string Prefix) : IFloorPlanMetricRule
{
    public List<string> Check(FloorPlanMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan metric.</summary>
public sealed record FloorPlanMetricValidator(List<IFloorPlanMetricRule> Rules)
{
    public List<string> Validate(FloorPlanMetric item)
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

    public void RequireValid(FloorPlanMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanMetric> items)
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

/// <summary>Functions of floor plan metric rules.</summary>
public static class FloorPlanMetricRules
{
    public static FloorPlanMetricValidator DefaultFloorPlanMetricValidator()
    {
        return new FloorPlanMetricValidator(
            new List<IFloorPlanMetricRule>
            {
                new FloorPlanMetricRequiredNameRule("floor plan metric needs a value"),
                new FloorPlanMetricRequiredCodeRule("floor plan metric needs a value"),
                new FloorPlanMetricRequiredTitleRule("floor plan metric needs an entry"),
                new FloorPlanMetricVisibleGuardRule(5.4),
                new FloorPlanMetricCeilingRangeRule(8.6, 3425.5),
                new FloorPlanMetricCodePrefixRule("X"),
            });
    }
}
