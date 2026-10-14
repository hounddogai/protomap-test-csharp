namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan request must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanRequestRule
{
    List<string> Check(FloorPlanRequest item);
}

public sealed record FloorPlanRequestSkillLengthRule(int MaxLength) : IFloorPlanRequestRule
{
    public List<string> Check(FloorPlanRequest item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanRequestRequiredLabelRule(string Message) : IFloorPlanRequestRule
{
    public List<string> Check(FloorPlanRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanRequestLabelLengthRule(int MaxLength) : IFloorPlanRequestRule
{
    public List<string> Check(FloorPlanRequest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanRequestRequiredCodeRule(string Message) : IFloorPlanRequestRule
{
    public List<string> Check(FloorPlanRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanRequestActiveGuardRule(double Limit) : IFloorPlanRequestRule
{
    public List<string> Check(FloorPlanRequest item)
    {
        if (item.Active && item.Ceiling > Limit)
        {
            return new List<string> { $"ceiling {item.Ceiling} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanRequestAllowedLabelRule(List<string> Allowed) : IFloorPlanRequestRule
{
    public List<string> Check(FloorPlanRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan request.</summary>
public sealed record FloorPlanRequestValidator(List<IFloorPlanRequestRule> Rules)
{
    public List<string> Validate(FloorPlanRequest item)
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

    public void RequireValid(FloorPlanRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanRequest> items)
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

/// <summary>Functions of floor plan request rules.</summary>
public static class FloorPlanRequestRules
{
    public static FloorPlanRequestValidator DefaultFloorPlanRequestValidator()
    {
        return new FloorPlanRequestValidator(
            new List<IFloorPlanRequestRule>
            {
                new FloorPlanRequestSkillLengthRule(64),
                new FloorPlanRequestRequiredLabelRule("floor plan request needs this field"),
                new FloorPlanRequestLabelLengthRule(32),
                new FloorPlanRequestRequiredCodeRule("floor plan request needs a value"),
                new FloorPlanRequestActiveGuardRule(4.5),
                new FloorPlanRequestAllowedLabelRule(new List<string>()),
            });
    }
}
