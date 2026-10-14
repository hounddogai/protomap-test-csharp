namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled floor plan snapshot must pass. Check returns its problems, or none.</summary>
public interface IScheduledFloorPlanSnapshotRule
{
    List<string> Check(ScheduledFloorPlanSnapshot item);
}

public sealed record ScheduledFloorPlanSnapshotCeilingRangeRule(double Min, double Max) : IScheduledFloorPlanSnapshotRule
{
    public List<string> Check(ScheduledFloorPlanSnapshot item)
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

public sealed record ScheduledFloorPlanSnapshotRequiredEmployeeIdRule(string Message) : IScheduledFloorPlanSnapshotRule
{
    public List<string> Check(ScheduledFloorPlanSnapshot item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFloorPlanSnapshotAllowedSalaryBandRule(List<string> Allowed) : IScheduledFloorPlanSnapshotRule
{
    public List<string> Check(ScheduledFloorPlanSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.SalaryBand)))
        {
            return new List<string> { $"salary band {item.SalaryBand} is stale" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFloorPlanSnapshotCodeLengthRule(int MaxLength) : IScheduledFloorPlanSnapshotRule
{
    public List<string> Check(ScheduledFloorPlanSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled floor plan snapshot.</summary>
public sealed record ScheduledFloorPlanSnapshotValidator(List<IScheduledFloorPlanSnapshotRule> Rules)
{
    public List<string> Validate(ScheduledFloorPlanSnapshot item)
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

    public void RequireValid(ScheduledFloorPlanSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledFloorPlanSnapshot> items)
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

/// <summary>Functions of scheduled floor plan snapshot rules.</summary>
public static class ScheduledFloorPlanSnapshotRules
{
    public static ScheduledFloorPlanSnapshotValidator DefaultScheduledFloorPlanSnapshotValidator()
    {
        return new ScheduledFloorPlanSnapshotValidator(
            new List<IScheduledFloorPlanSnapshotRule>
            {
                new ScheduledFloorPlanSnapshotCeilingRangeRule(9.5, 3986.3),
                new ScheduledFloorPlanSnapshotRequiredEmployeeIdRule(
                    "scheduled floor plan snapshot needs this field"),
                new ScheduledFloorPlanSnapshotAllowedSalaryBandRule(new List<string> { "capped", "batched" }),
                new ScheduledFloorPlanSnapshotCodeLengthRule(32),
            });
    }
}
