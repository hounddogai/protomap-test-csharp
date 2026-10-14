namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>A rule a adaptive floor plan adjustment must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveFloorPlanAdjustmentRule
{
    List<string> Check(AdaptiveFloorPlanAdjustment item);
}

public sealed record AdaptiveFloorPlanAdjustmentLabelLengthRule(int MaxLength) : IAdaptiveFloorPlanAdjustmentRule
{
    public List<string> Check(AdaptiveFloorPlanAdjustment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFloorPlanAdjustmentCodeLengthRule(int MaxLength) : IAdaptiveFloorPlanAdjustmentRule
{
    public List<string> Check(AdaptiveFloorPlanAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFloorPlanAdjustmentRequiredLabelRule(string Message) : IAdaptiveFloorPlanAdjustmentRule
{
    public List<string> Check(AdaptiveFloorPlanAdjustment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFloorPlanAdjustmentCodePrefixRule(string Prefix) : IAdaptiveFloorPlanAdjustmentRule
{
    public List<string> Check(AdaptiveFloorPlanAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFloorPlanAdjustmentNameLengthRule(int MaxLength) : IAdaptiveFloorPlanAdjustmentRule
{
    public List<string> Check(AdaptiveFloorPlanAdjustment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFloorPlanAdjustmentDepartmentLengthRule(int MaxLength) : IAdaptiveFloorPlanAdjustmentRule
{
    public List<string> Check(AdaptiveFloorPlanAdjustment item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive floor plan adjustment.</summary>
public sealed record AdaptiveFloorPlanAdjustmentValidator(List<IAdaptiveFloorPlanAdjustmentRule> Rules)
{
    public List<string> Validate(AdaptiveFloorPlanAdjustment item)
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

    public void RequireValid(AdaptiveFloorPlanAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveFloorPlanAdjustment> items)
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

/// <summary>Functions of adaptive floor plan adjustment rules.</summary>
public static class AdaptiveFloorPlanAdjustmentRules
{
    public static AdaptiveFloorPlanAdjustmentValidator DefaultAdaptiveFloorPlanAdjustmentValidator()
    {
        return new AdaptiveFloorPlanAdjustmentValidator(
            new List<IAdaptiveFloorPlanAdjustmentRule>
            {
                new AdaptiveFloorPlanAdjustmentLabelLengthRule(16),
                new AdaptiveFloorPlanAdjustmentCodeLengthRule(1000),
                new AdaptiveFloorPlanAdjustmentRequiredLabelRule("adaptive floor plan adjustment needs a value"),
                new AdaptiveFloorPlanAdjustmentCodePrefixRule("Q"),
                new AdaptiveFloorPlanAdjustmentNameLengthRule(86400),
                new AdaptiveFloorPlanAdjustmentDepartmentLengthRule(80),
            });
    }
}
