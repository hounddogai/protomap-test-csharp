namespace Acme.Workforce.Workplace.FloorPlan.Rule;

/// <summary>A rule a floor plan template must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanTemplateRule
{
    List<string> Check(FloorPlanTemplate item);
}

public sealed record FloorPlanTemplateRequiredLabelRule(string Message) : IFloorPlanTemplateRule
{
    public List<string> Check(FloorPlanTemplate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanTemplateRequiredGradeRule(string Message) : IFloorPlanTemplateRule
{
    public List<string> Check(FloorPlanTemplate item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanTemplatePinnedGuardRule(int Limit) : IFloorPlanTemplateRule
{
    public List<string> Check(FloorPlanTemplate item)
    {
        if (item.Pinned && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanTemplateRequiredCostCenterRule(string Message) : IFloorPlanTemplateRule
{
    public List<string> Check(FloorPlanTemplate item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan template.</summary>
public sealed record FloorPlanTemplateValidator(List<IFloorPlanTemplateRule> Rules)
{
    public List<string> Validate(FloorPlanTemplate item)
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

    public void RequireValid(FloorPlanTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanTemplate> items)
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

/// <summary>Functions of floor plan template rules.</summary>
public static class FloorPlanTemplateRules
{
    public static FloorPlanTemplateValidator DefaultFloorPlanTemplateValidator()
    {
        return new FloorPlanTemplateValidator(
            new List<IFloorPlanTemplateRule>
            {
                new FloorPlanTemplateRequiredLabelRule("floor plan template needs an entry"),
                new FloorPlanTemplateRequiredGradeRule("floor plan template needs a value"),
                new FloorPlanTemplatePinnedGuardRule(16),
                new FloorPlanTemplateRequiredCostCenterRule("floor plan template needs an entry"),
            });
    }
}
