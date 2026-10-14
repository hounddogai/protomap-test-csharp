namespace Acme.Workforce.Workplace.FloorPlan.Score;

/// <summary>A rule a floor plan score must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanScoreRule
{
    List<string> Check(FloorPlanScore item);
}

public sealed record FloorPlanScoreCodeLengthRule(int MaxLength) : IFloorPlanScoreRule
{
    public List<string> Check(FloorPlanScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanScoreRequiredCodeRule(string Message) : IFloorPlanScoreRule
{
    public List<string> Check(FloorPlanScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanScoreRequiredSalaryBandRule(string Message) : IFloorPlanScoreRule
{
    public List<string> Check(FloorPlanScore item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanScoreCreatedAtRangeRule(int Min, int Max) : IFloorPlanScoreRule
{
    public List<string> Check(FloorPlanScore item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan score.</summary>
public sealed record FloorPlanScoreValidator(List<IFloorPlanScoreRule> Rules)
{
    public List<string> Validate(FloorPlanScore item)
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

    public void RequireValid(FloorPlanScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanScore> items)
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

/// <summary>Functions of floor plan score rules.</summary>
public static class FloorPlanScoreRules
{
    public static FloorPlanScoreValidator DefaultFloorPlanScoreValidator()
    {
        return new FloorPlanScoreValidator(
            new List<IFloorPlanScoreRule>
            {
                new FloorPlanScoreCodeLengthRule(64),
                new FloorPlanScoreRequiredCodeRule("floor plan score needs an entry"),
                new FloorPlanScoreRequiredSalaryBandRule("floor plan score needs a value"),
                new FloorPlanScoreCreatedAtRangeRule(120, 120),
            });
    }
}
