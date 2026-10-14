namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>A rule a floor plan allocation must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanAllocationRule
{
    List<string> Check(FloorPlanAllocation item);
}

public sealed record FloorPlanAllocationRequiredGradeRule(string Message) : IFloorPlanAllocationRule
{
    public List<string> Check(FloorPlanAllocation item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAllocationRequiredCodeRule(string Message) : IFloorPlanAllocationRule
{
    public List<string> Check(FloorPlanAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAllocationRequiredLabelRule(string Message) : IFloorPlanAllocationRule
{
    public List<string> Check(FloorPlanAllocation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAllocationRevisionRangeRule(int Min, int Max) : IFloorPlanAllocationRule
{
    public List<string> Check(FloorPlanAllocation item)
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

public sealed record FloorPlanAllocationLabelLengthRule(int MaxLength) : IFloorPlanAllocationRule
{
    public List<string> Check(FloorPlanAllocation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan allocation.</summary>
public sealed record FloorPlanAllocationValidator(List<IFloorPlanAllocationRule> Rules)
{
    public List<string> Validate(FloorPlanAllocation item)
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

    public void RequireValid(FloorPlanAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanAllocation> items)
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

/// <summary>Functions of floor plan allocation rules.</summary>
public static class FloorPlanAllocationRules
{
    public static FloorPlanAllocationValidator DefaultFloorPlanAllocationValidator()
    {
        return new FloorPlanAllocationValidator(
            new List<IFloorPlanAllocationRule>
            {
                new FloorPlanAllocationRequiredGradeRule("floor plan allocation needs an entry"),
                new FloorPlanAllocationRequiredCodeRule("floor plan allocation needs this field"),
                new FloorPlanAllocationRequiredLabelRule("floor plan allocation needs this field"),
                new FloorPlanAllocationRevisionRangeRule(80, 255),
                new FloorPlanAllocationLabelLengthRule(86400),
            });
    }
}
