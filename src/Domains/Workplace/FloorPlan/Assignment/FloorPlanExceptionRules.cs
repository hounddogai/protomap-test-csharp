namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

/// <summary>A rule a floor plan exception must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanExceptionRule
{
    List<string> Check(FloorPlanException item);
}

public sealed record FloorPlanExceptionApprovedGuardRule(int Limit) : IFloorPlanExceptionRule
{
    public List<string> Check(FloorPlanException item)
    {
        if (item.Approved && item.GraceDays > Limit)
        {
            return new List<string> { $"grace days {item.GraceDays} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanExceptionLeaveDaysRangeRule(int Min, int Max) : IFloorPlanExceptionRule
{
    public List<string> Check(FloorPlanException item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanExceptionCodeLengthRule(int MaxLength) : IFloorPlanExceptionRule
{
    public List<string> Check(FloorPlanException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan exception.</summary>
public sealed record FloorPlanExceptionValidator(List<IFloorPlanExceptionRule> Rules)
{
    public List<string> Validate(FloorPlanException item)
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

    public void RequireValid(FloorPlanException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanException> items)
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

/// <summary>Functions of floor plan exception rules.</summary>
public static class FloorPlanExceptionRules
{
    public static FloorPlanExceptionValidator DefaultFloorPlanExceptionValidator()
    {
        return new FloorPlanExceptionValidator(
            new List<IFloorPlanExceptionRule>
            {
                new FloorPlanExceptionApprovedGuardRule(64),
                new FloorPlanExceptionLeaveDaysRangeRule(16, 1000),
                new FloorPlanExceptionCodeLengthRule(255),
            });
    }
}
