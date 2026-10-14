namespace Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>A rule a floor plan cycle must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanCycleRule
{
    List<string> Check(FloorPlanCycle item);
}

public sealed record FloorPlanCycleLabelLengthRule(int MaxLength) : IFloorPlanCycleRule
{
    public List<string> Check(FloorPlanCycle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanCycleRevisionRangeRule(int Min, int Max) : IFloorPlanCycleRule
{
    public List<string> Check(FloorPlanCycle item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FloorPlanCycleCodePrefixRule(string Prefix) : IFloorPlanCycleRule
{
    public List<string> Check(FloorPlanCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanCycleCapacityRangeRule(int Min, int Max) : IFloorPlanCycleRule
{
    public List<string> Check(FloorPlanCycle item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanCycleFloorValueRangeRule(double Min, double Max) : IFloorPlanCycleRule
{
    public List<string> Check(FloorPlanCycle item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FloorPlanCycleRequiredCodeRule(string Message) : IFloorPlanCycleRule
{
    public List<string> Check(FloorPlanCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan cycle.</summary>
public sealed record FloorPlanCycleValidator(List<IFloorPlanCycleRule> Rules)
{
    public List<string> Validate(FloorPlanCycle item)
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

    public void RequireValid(FloorPlanCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanCycle> items)
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

/// <summary>Functions of floor plan cycle rules.</summary>
public static class FloorPlanCycleRules
{
    public static FloorPlanCycleValidator DefaultFloorPlanCycleValidator()
    {
        return new FloorPlanCycleValidator(
            new List<IFloorPlanCycleRule>
            {
                new FloorPlanCycleLabelLengthRule(3600),
                new FloorPlanCycleRevisionRangeRule(255, 86400),
                new FloorPlanCycleCodePrefixRule("X"),
                new FloorPlanCycleCapacityRangeRule(16, 86400),
                new FloorPlanCycleFloorValueRangeRule(1.5, 830.5),
                new FloorPlanCycleRequiredCodeRule("floor plan cycle needs a value"),
            });
    }
}
