namespace Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>A rule a strict occupancy adjustment must pass. Check returns its problems, or none.</summary>
public interface IStrictOccupancyAdjustmentRule
{
    List<string> Check(StrictOccupancyAdjustment item);
}

public sealed record StrictOccupancyAdjustmentRequiredDepartmentRule(string Message) : IStrictOccupancyAdjustmentRule
{
    public List<string> Check(StrictOccupancyAdjustment item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancyAdjustmentDepartmentLengthRule(int MaxLength) : IStrictOccupancyAdjustmentRule
{
    public List<string> Check(StrictOccupancyAdjustment item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancyAdjustmentDurationMinutesRangeRule(int Min, int Max) : IStrictOccupancyAdjustmentRule
{
    public List<string> Check(StrictOccupancyAdjustment item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictOccupancyAdjustmentBudgetRangeRule(double Min, double Max) : IStrictOccupancyAdjustmentRule
{
    public List<string> Check(StrictOccupancyAdjustment item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancyAdjustmentRequiredLabelRule(string Message) : IStrictOccupancyAdjustmentRule
{
    public List<string> Check(StrictOccupancyAdjustment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict occupancy adjustment.</summary>
public sealed record StrictOccupancyAdjustmentValidator(List<IStrictOccupancyAdjustmentRule> Rules)
{
    public List<string> Validate(StrictOccupancyAdjustment item)
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

    public void RequireValid(StrictOccupancyAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictOccupancyAdjustment> items)
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

/// <summary>Functions of strict occupancy adjustment rules.</summary>
public static class StrictOccupancyAdjustmentRules
{
    public static StrictOccupancyAdjustmentValidator DefaultStrictOccupancyAdjustmentValidator()
    {
        return new StrictOccupancyAdjustmentValidator(
            new List<IStrictOccupancyAdjustmentRule>
            {
                new StrictOccupancyAdjustmentRequiredDepartmentRule("strict occupancy adjustment needs a value"),
                new StrictOccupancyAdjustmentDepartmentLengthRule(3600),
                new StrictOccupancyAdjustmentDurationMinutesRangeRule(120, 1000),
                new StrictOccupancyAdjustmentBudgetRangeRule(0.8, 2160.5),
                new StrictOccupancyAdjustmentRequiredLabelRule("strict occupancy adjustment needs this field"),
            });
    }
}
