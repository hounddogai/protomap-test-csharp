namespace Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>A rule a scheduled floor plan target must pass. Check returns its problems, or none.</summary>
public interface IScheduledFloorPlanTargetRule
{
    List<string> Check(ScheduledFloorPlanTarget item);
}

public sealed record ScheduledFloorPlanTargetCategoryLengthRule(int MaxLength) : IScheduledFloorPlanTargetRule
{
    public List<string> Check(ScheduledFloorPlanTarget item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFloorPlanTargetRequiredCodeRule(string Message) : IScheduledFloorPlanTargetRule
{
    public List<string> Check(ScheduledFloorPlanTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFloorPlanTargetCodeLengthRule(int MaxLength) : IScheduledFloorPlanTargetRule
{
    public List<string> Check(ScheduledFloorPlanTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled floor plan target.</summary>
public sealed record ScheduledFloorPlanTargetValidator(List<IScheduledFloorPlanTargetRule> Rules)
{
    public List<string> Validate(ScheduledFloorPlanTarget item)
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

    public void RequireValid(ScheduledFloorPlanTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledFloorPlanTarget> items)
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

/// <summary>Functions of scheduled floor plan target rules.</summary>
public static class ScheduledFloorPlanTargetRules
{
    public static ScheduledFloorPlanTargetValidator DefaultScheduledFloorPlanTargetValidator()
    {
        return new ScheduledFloorPlanTargetValidator(
            new List<IScheduledFloorPlanTargetRule>
            {
                new ScheduledFloorPlanTargetCategoryLengthRule(1000),
                new ScheduledFloorPlanTargetRequiredCodeRule("scheduled floor plan target needs an entry"),
                new ScheduledFloorPlanTargetCodeLengthRule(32),
            });
    }
}
