namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan record must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanRecordRule
{
    List<string> Check(FloorPlanRecord item);
}

public sealed record FloorPlanRecordSourceLengthRule(int MaxLength) : IFloorPlanRecordRule
{
    public List<string> Check(FloorPlanRecord item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanRecordNameLengthRule(int MaxLength) : IFloorPlanRecordRule
{
    public List<string> Check(FloorPlanRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanRecordAllowedSourceRule(List<string> Allowed) : IFloorPlanRecordRule
{
    public List<string> Check(FloorPlanRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan record.</summary>
public sealed record FloorPlanRecordValidator(List<IFloorPlanRecordRule> Rules)
{
    public List<string> Validate(FloorPlanRecord item)
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

    public void RequireValid(FloorPlanRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanRecord> items)
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

/// <summary>Functions of floor plan record rules.</summary>
public static class FloorPlanRecordRules
{
    public static FloorPlanRecordValidator DefaultFloorPlanRecordValidator()
    {
        return new FloorPlanRecordValidator(
            new List<IFloorPlanRecordRule>
            {
                new FloorPlanRecordSourceLengthRule(32),
                new FloorPlanRecordNameLengthRule(80),
                new FloorPlanRecordAllowedSourceRule(new List<string> { "regional", "default", "capped" }),
            });
    }
}
