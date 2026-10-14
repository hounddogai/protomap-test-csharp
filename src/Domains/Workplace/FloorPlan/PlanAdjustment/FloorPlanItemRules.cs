namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>A rule a floor plan item must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanItemRule
{
    List<string> Check(FloorPlanItem item);
}

public sealed record FloorPlanItemCodeLengthRule(int MaxLength) : IFloorPlanItemRule
{
    public List<string> Check(FloorPlanItem item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanItemApprovedGuardRule(double Limit) : IFloorPlanItemRule
{
    public List<string> Check(FloorPlanItem item)
    {
        if (item.Approved && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanItemNameLengthRule(int MaxLength) : IFloorPlanItemRule
{
    public List<string> Check(FloorPlanItem item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan item.</summary>
public sealed record FloorPlanItemValidator(List<IFloorPlanItemRule> Rules)
{
    public List<string> Validate(FloorPlanItem item)
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

    public void RequireValid(FloorPlanItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanItem> items)
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

/// <summary>Functions of floor plan item rules.</summary>
public static class FloorPlanItemRules
{
    public static FloorPlanItemValidator DefaultFloorPlanItemValidator()
    {
        return new FloorPlanItemValidator(
            new List<IFloorPlanItemRule>
            {
                new FloorPlanItemCodeLengthRule(16),
                new FloorPlanItemApprovedGuardRule(7.4),
                new FloorPlanItemNameLengthRule(64),
            });
    }
}
