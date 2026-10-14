namespace Acme.Workforce.Workplace.FloorPlan.Tier;

/// <summary>A rule a floor plan rollup must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanRollupRule
{
    List<string> Check(FloorPlanRollup item);
}

public sealed record FloorPlanRollupRequiredSalaryBandRule(string Message) : IFloorPlanRollupRule
{
    public List<string> Check(FloorPlanRollup item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanRollupRequiredJobCodeRule(string Message) : IFloorPlanRollupRule
{
    public List<string> Check(FloorPlanRollup item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanRollupCodePrefixRule(string Prefix) : IFloorPlanRollupRule
{
    public List<string> Check(FloorPlanRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanRollupRequiredCodeRule(string Message) : IFloorPlanRollupRule
{
    public List<string> Check(FloorPlanRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan rollup.</summary>
public sealed record FloorPlanRollupValidator(List<IFloorPlanRollupRule> Rules)
{
    public List<string> Validate(FloorPlanRollup item)
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

    public void RequireValid(FloorPlanRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanRollup> items)
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

/// <summary>Functions of floor plan rollup rules.</summary>
public static class FloorPlanRollupRules
{
    public static FloorPlanRollupValidator DefaultFloorPlanRollupValidator()
    {
        return new FloorPlanRollupValidator(
            new List<IFloorPlanRollupRule>
            {
                new FloorPlanRollupRequiredSalaryBandRule("floor plan rollup needs an entry"),
                new FloorPlanRollupRequiredJobCodeRule("floor plan rollup needs a value"),
                new FloorPlanRollupCodePrefixRule("REF"),
                new FloorPlanRollupRequiredCodeRule("floor plan rollup needs a value"),
            });
    }
}
