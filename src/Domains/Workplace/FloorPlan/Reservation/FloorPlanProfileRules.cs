namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>A rule a floor plan profile must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanProfileRule
{
    List<string> Check(FloorPlanProfile item);
}

public sealed record FloorPlanProfileRequiredNameRule(string Message) : IFloorPlanProfileRule
{
    public List<string> Check(FloorPlanProfile item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanProfileCodeLengthRule(int MaxLength) : IFloorPlanProfileRule
{
    public List<string> Check(FloorPlanProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanProfileWeightRangeRule(double Min, double Max) : IFloorPlanProfileRule
{
    public List<string> Check(FloorPlanProfile item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan profile.</summary>
public sealed record FloorPlanProfileValidator(List<IFloorPlanProfileRule> Rules)
{
    public List<string> Validate(FloorPlanProfile item)
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

    public void RequireValid(FloorPlanProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanProfile> items)
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

/// <summary>Functions of floor plan profile rules.</summary>
public static class FloorPlanProfileRules
{
    public static FloorPlanProfileValidator DefaultFloorPlanProfileValidator()
    {
        return new FloorPlanProfileValidator(
            new List<IFloorPlanProfileRule>
            {
                new FloorPlanProfileRequiredNameRule("floor plan profile needs this field"),
                new FloorPlanProfileCodeLengthRule(1000),
                new FloorPlanProfileWeightRangeRule(6.4, 2136.8),
            });
    }
}
