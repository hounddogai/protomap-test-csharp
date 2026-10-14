namespace Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>A rule a floor plan entry must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanEntryRule
{
    List<string> Check(FloorPlanEntry item);
}

public sealed record FloorPlanEntryRequiredNameRule(string Message) : IFloorPlanEntryRule
{
    public List<string> Check(FloorPlanEntry item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanEntryRequiredCodeRule(string Message) : IFloorPlanEntryRule
{
    public List<string> Check(FloorPlanEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanEntryRevisionRangeRule(int Min, int Max) : IFloorPlanEntryRule
{
    public List<string> Check(FloorPlanEntry item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanEntryRequiredTitleRule(string Message) : IFloorPlanEntryRule
{
    public List<string> Check(FloorPlanEntry item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanEntryCodePrefixRule(string Prefix) : IFloorPlanEntryRule
{
    public List<string> Check(FloorPlanEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan entry.</summary>
public sealed record FloorPlanEntryValidator(List<IFloorPlanEntryRule> Rules)
{
    public List<string> Validate(FloorPlanEntry item)
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

    public void RequireValid(FloorPlanEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanEntry> items)
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

/// <summary>Functions of floor plan entry rules.</summary>
public static class FloorPlanEntryRules
{
    public static FloorPlanEntryValidator DefaultFloorPlanEntryValidator()
    {
        return new FloorPlanEntryValidator(
            new List<IFloorPlanEntryRule>
            {
                new FloorPlanEntryRequiredNameRule("floor plan entry needs a value"),
                new FloorPlanEntryRequiredCodeRule("floor plan entry needs a value"),
                new FloorPlanEntryRevisionRangeRule(255, 86400),
                new FloorPlanEntryRequiredTitleRule("floor plan entry needs an entry"),
                new FloorPlanEntryCodePrefixRule("X"),
            });
    }
}
