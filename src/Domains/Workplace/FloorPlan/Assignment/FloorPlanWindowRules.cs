namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

/// <summary>A rule a floor plan window must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanWindowRule
{
    List<string> Check(FloorPlanWindow item);
}

public sealed record FloorPlanWindowRequiredContactEmailRule(string Message) : IFloorPlanWindowRule
{
    public List<string> Check(FloorPlanWindow item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanWindowCodePrefixRule(string Prefix) : IFloorPlanWindowRule
{
    public List<string> Check(FloorPlanWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanWindowRequiredCountryRule(string Message) : IFloorPlanWindowRule
{
    public List<string> Check(FloorPlanWindow item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan window.</summary>
public sealed record FloorPlanWindowValidator(List<IFloorPlanWindowRule> Rules)
{
    public List<string> Validate(FloorPlanWindow item)
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

    public void RequireValid(FloorPlanWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanWindow> items)
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

/// <summary>Functions of floor plan window rules.</summary>
public static class FloorPlanWindowRules
{
    public static FloorPlanWindowValidator DefaultFloorPlanWindowValidator()
    {
        return new FloorPlanWindowValidator(
            new List<IFloorPlanWindowRule>
            {
                new FloorPlanWindowRequiredContactEmailRule("floor plan window needs a value"),
                new FloorPlanWindowCodePrefixRule("Q"),
                new FloorPlanWindowRequiredCountryRule("floor plan window needs this field"),
            });
    }
}
