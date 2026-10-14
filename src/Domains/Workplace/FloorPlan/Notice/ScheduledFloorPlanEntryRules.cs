namespace Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>A rule a scheduled floor plan entry must pass. Check returns its problems, or none.</summary>
public interface IScheduledFloorPlanEntryRule
{
    List<string> Check(ScheduledFloorPlanEntry item);
}

public sealed record ScheduledFloorPlanEntryEndsAtRangeRule(int Min, int Max) : IScheduledFloorPlanEntryRule
{
    public List<string> Check(ScheduledFloorPlanEntry item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFloorPlanEntryRequiredCodeRule(string Message) : IScheduledFloorPlanEntryRule
{
    public List<string> Check(ScheduledFloorPlanEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFloorPlanEntryRequiredSegmentRule(string Message) : IScheduledFloorPlanEntryRule
{
    public List<string> Check(ScheduledFloorPlanEntry item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFloorPlanEntryExpiresAtRangeRule(int Min, int Max) : IScheduledFloorPlanEntryRule
{
    public List<string> Check(ScheduledFloorPlanEntry item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFloorPlanEntryWeightRangeRule(double Min, double Max) : IScheduledFloorPlanEntryRule
{
    public List<string> Check(ScheduledFloorPlanEntry item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled floor plan entry.</summary>
public sealed record ScheduledFloorPlanEntryValidator(List<IScheduledFloorPlanEntryRule> Rules)
{
    public List<string> Validate(ScheduledFloorPlanEntry item)
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

    public void RequireValid(ScheduledFloorPlanEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledFloorPlanEntry> items)
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

/// <summary>Functions of scheduled floor plan entry rules.</summary>
public static class ScheduledFloorPlanEntryRules
{
    public static ScheduledFloorPlanEntryValidator DefaultScheduledFloorPlanEntryValidator()
    {
        return new ScheduledFloorPlanEntryValidator(
            new List<IScheduledFloorPlanEntryRule>
            {
                new ScheduledFloorPlanEntryEndsAtRangeRule(80, 86400),
                new ScheduledFloorPlanEntryRequiredCodeRule("scheduled floor plan entry needs a value"),
                new ScheduledFloorPlanEntryRequiredSegmentRule("scheduled floor plan entry needs a value"),
                new ScheduledFloorPlanEntryExpiresAtRangeRule(16, 1000),
                new ScheduledFloorPlanEntryWeightRangeRule(5.9, 2657.5),
            });
    }
}
