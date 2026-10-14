namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>A rule a floor plan override must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanOverrideRule
{
    List<string> Check(FloorPlanOverride item);
}

public sealed record FloorPlanOverrideLabelLengthRule(int MaxLength) : IFloorPlanOverrideRule
{
    public List<string> Check(FloorPlanOverride item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanOverrideRequiredCodeRule(string Message) : IFloorPlanOverrideRule
{
    public List<string> Check(FloorPlanOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanOverrideChannelLengthRule(int MaxLength) : IFloorPlanOverrideRule
{
    public List<string> Check(FloorPlanOverride item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan override.</summary>
public sealed record FloorPlanOverrideValidator(List<IFloorPlanOverrideRule> Rules)
{
    public List<string> Validate(FloorPlanOverride item)
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

    public void RequireValid(FloorPlanOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanOverride> items)
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

/// <summary>Functions of floor plan override rules.</summary>
public static class FloorPlanOverrideRules
{
    public static FloorPlanOverrideValidator DefaultFloorPlanOverrideValidator()
    {
        return new FloorPlanOverrideValidator(
            new List<IFloorPlanOverrideRule>
            {
                new FloorPlanOverrideLabelLengthRule(16),
                new FloorPlanOverrideRequiredCodeRule("floor plan override needs a value"),
                new FloorPlanOverrideChannelLengthRule(3600),
            });
    }
}
