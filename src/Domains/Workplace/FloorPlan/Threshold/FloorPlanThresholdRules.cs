namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>A rule a floor plan threshold must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanThresholdRule
{
    List<string> Check(FloorPlanThreshold item);
}

public sealed record FloorPlanThresholdCodePrefixRule(string Prefix) : IFloorPlanThresholdRule
{
    public List<string> Check(FloorPlanThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanThresholdEndsAtRangeRule(int Min, int Max) : IFloorPlanThresholdRule
{
    public List<string> Check(FloorPlanThreshold item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanThresholdWindowRule(int MaxSpan) : IFloorPlanThresholdRule
{
    public List<string> Check(FloorPlanThreshold item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanThresholdRequiredSalaryBandRule(string Message) : IFloorPlanThresholdRule
{
    public List<string> Check(FloorPlanThreshold item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanThresholdRequiredLocationRule(string Message) : IFloorPlanThresholdRule
{
    public List<string> Check(FloorPlanThreshold item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanThresholdArchivedGuardRule(int Limit) : IFloorPlanThresholdRule
{
    public List<string> Check(FloorPlanThreshold item)
    {
        if (item.Archived && item.GraceDays > Limit)
        {
            return new List<string> { $"grace days {item.GraceDays} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan threshold.</summary>
public sealed record FloorPlanThresholdValidator(List<IFloorPlanThresholdRule> Rules)
{
    public List<string> Validate(FloorPlanThreshold item)
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

    public void RequireValid(FloorPlanThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanThreshold> items)
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

/// <summary>Functions of floor plan threshold rules.</summary>
public static class FloorPlanThresholdRules
{
    public static FloorPlanThresholdValidator DefaultFloorPlanThresholdValidator()
    {
        return new FloorPlanThresholdValidator(
            new List<IFloorPlanThresholdRule>
            {
                new FloorPlanThresholdCodePrefixRule("OPS"),
                new FloorPlanThresholdEndsAtRangeRule(255, 255),
                new FloorPlanThresholdWindowRule(32),
                new FloorPlanThresholdRequiredSalaryBandRule("floor plan threshold needs this field"),
                new FloorPlanThresholdRequiredLocationRule("floor plan threshold needs a value"),
                new FloorPlanThresholdArchivedGuardRule(120),
            });
    }
}
