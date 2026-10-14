namespace Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>A rule a rolling floor plan rollup must pass. Check returns its problems, or none.</summary>
public interface IRollingFloorPlanRollupRule
{
    List<string> Check(RollingFloorPlanRollup item);
}

public sealed record RollingFloorPlanRollupNameLengthRule(int MaxLength) : IRollingFloorPlanRollupRule
{
    public List<string> Check(RollingFloorPlanRollup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanRollupRatingRangeRule(double Min, double Max) : IRollingFloorPlanRollupRule
{
    public List<string> Check(RollingFloorPlanRollup item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingFloorPlanRollupMarginRangeRule(double Min, double Max) : IRollingFloorPlanRollupRule
{
    public List<string> Check(RollingFloorPlanRollup item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingFloorPlanRollupFloorValueRangeRule(double Min, double Max) : IRollingFloorPlanRollupRule
{
    public List<string> Check(RollingFloorPlanRollup item)
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

public sealed record RollingFloorPlanRollupCodeLengthRule(int MaxLength) : IRollingFloorPlanRollupRule
{
    public List<string> Check(RollingFloorPlanRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanRollupRequiredNameRule(string Message) : IRollingFloorPlanRollupRule
{
    public List<string> Check(RollingFloorPlanRollup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling floor plan rollup.</summary>
public sealed record RollingFloorPlanRollupValidator(List<IRollingFloorPlanRollupRule> Rules)
{
    public List<string> Validate(RollingFloorPlanRollup item)
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

    public void RequireValid(RollingFloorPlanRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingFloorPlanRollup> items)
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

/// <summary>Functions of rolling floor plan rollup rules.</summary>
public static class RollingFloorPlanRollupRules
{
    public static RollingFloorPlanRollupValidator DefaultRollingFloorPlanRollupValidator()
    {
        return new RollingFloorPlanRollupValidator(
            new List<IRollingFloorPlanRollupRule>
            {
                new RollingFloorPlanRollupNameLengthRule(3600),
                new RollingFloorPlanRollupRatingRangeRule(3.1, 3485.1),
                new RollingFloorPlanRollupMarginRangeRule(10.0, 2646.2),
                new RollingFloorPlanRollupFloorValueRangeRule(5.6, 2531.8),
                new RollingFloorPlanRollupCodeLengthRule(1000),
                new RollingFloorPlanRollupRequiredNameRule("rolling floor plan rollup needs this field"),
            });
    }
}
