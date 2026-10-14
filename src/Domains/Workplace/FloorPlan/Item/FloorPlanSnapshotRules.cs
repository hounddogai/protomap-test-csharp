namespace Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>A rule a floor plan snapshot must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanSnapshotRule
{
    List<string> Check(FloorPlanSnapshot item);
}

public sealed record FloorPlanSnapshotMaximumRangeRule(double Min, double Max) : IFloorPlanSnapshotRule
{
    public List<string> Check(FloorPlanSnapshot item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FloorPlanSnapshotCodeLengthRule(int MaxLength) : IFloorPlanSnapshotRule
{
    public List<string> Check(FloorPlanSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanSnapshotRequiredSourceRule(string Message) : IFloorPlanSnapshotRule
{
    public List<string> Check(FloorPlanSnapshot item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan snapshot.</summary>
public sealed record FloorPlanSnapshotValidator(List<IFloorPlanSnapshotRule> Rules)
{
    public List<string> Validate(FloorPlanSnapshot item)
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

    public void RequireValid(FloorPlanSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanSnapshot> items)
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

/// <summary>Functions of floor plan snapshot rules.</summary>
public static class FloorPlanSnapshotRules
{
    public static FloorPlanSnapshotValidator DefaultFloorPlanSnapshotValidator()
    {
        return new FloorPlanSnapshotValidator(
            new List<IFloorPlanSnapshotRule>
            {
                new FloorPlanSnapshotMaximumRangeRule(5.0, 3508.0),
                new FloorPlanSnapshotCodeLengthRule(16),
                new FloorPlanSnapshotRequiredSourceRule("floor plan snapshot needs this field"),
            });
    }
}
