namespace Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>A rule a floor plan slot must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanSlotRule
{
    List<string> Check(FloorPlanSlot item);
}

public sealed record FloorPlanSlotCurrencyLengthRule(int MaxLength) : IFloorPlanSlotRule
{
    public List<string> Check(FloorPlanSlot item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanSlotEmployeeIdLengthRule(int MaxLength) : IFloorPlanSlotRule
{
    public List<string> Check(FloorPlanSlot item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanSlotCodeLengthRule(int MaxLength) : IFloorPlanSlotRule
{
    public List<string> Check(FloorPlanSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanSlotEndsAtRangeRule(int Min, int Max) : IFloorPlanSlotRule
{
    public List<string> Check(FloorPlanSlot item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FloorPlanSlotBudgetRangeRule(double Min, double Max) : IFloorPlanSlotRule
{
    public List<string> Check(FloorPlanSlot item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FloorPlanSlotRequiredEmployeeIdRule(string Message) : IFloorPlanSlotRule
{
    public List<string> Check(FloorPlanSlot item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan slot.</summary>
public sealed record FloorPlanSlotValidator(List<IFloorPlanSlotRule> Rules)
{
    public List<string> Validate(FloorPlanSlot item)
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

    public void RequireValid(FloorPlanSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanSlot> items)
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

/// <summary>Functions of floor plan slot rules.</summary>
public static class FloorPlanSlotRules
{
    public static FloorPlanSlotValidator DefaultFloorPlanSlotValidator()
    {
        return new FloorPlanSlotValidator(
            new List<IFloorPlanSlotRule>
            {
                new FloorPlanSlotCurrencyLengthRule(80),
                new FloorPlanSlotEmployeeIdLengthRule(3600),
                new FloorPlanSlotCodeLengthRule(3600),
                new FloorPlanSlotEndsAtRangeRule(120, 3600),
                new FloorPlanSlotBudgetRangeRule(0.5, 1199.9),
                new FloorPlanSlotRequiredEmployeeIdRule("floor plan slot needs a value"),
            });
    }
}
