namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan adjustment must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanAdjustmentRule
{
    List<string> Check(FloorPlanAdjustment item);
}

public sealed record FloorPlanAdjustmentRequiredPhoneRule(string Message) : IFloorPlanAdjustmentRule
{
    public List<string> Check(FloorPlanAdjustment item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAdjustmentGradeLengthRule(int MaxLength) : IFloorPlanAdjustmentRule
{
    public List<string> Check(FloorPlanAdjustment item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAdjustmentCodePrefixRule(string Prefix) : IFloorPlanAdjustmentRule
{
    public List<string> Check(FloorPlanAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAdjustmentAllowedGradeRule(List<string> Allowed) : IFloorPlanAdjustmentRule
{
    public List<string> Check(FloorPlanAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAdjustmentRequiredGradeRule(string Message) : IFloorPlanAdjustmentRule
{
    public List<string> Check(FloorPlanAdjustment item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan adjustment.</summary>
public sealed record FloorPlanAdjustmentValidator(List<IFloorPlanAdjustmentRule> Rules)
{
    public List<string> Validate(FloorPlanAdjustment item)
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

    public void RequireValid(FloorPlanAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanAdjustment> items)
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

/// <summary>Functions of floor plan adjustment rules.</summary>
public static class FloorPlanAdjustmentRules
{
    public static FloorPlanAdjustmentValidator DefaultFloorPlanAdjustmentValidator()
    {
        return new FloorPlanAdjustmentValidator(
            new List<IFloorPlanAdjustmentRule>
            {
                new FloorPlanAdjustmentRequiredPhoneRule("floor plan adjustment needs an entry"),
                new FloorPlanAdjustmentGradeLengthRule(86400),
                new FloorPlanAdjustmentCodePrefixRule("OPS"),
                new FloorPlanAdjustmentAllowedGradeRule(new List<string> { "default", "legacy" }),
                new FloorPlanAdjustmentRequiredGradeRule("floor plan adjustment needs this field"),
            });
    }
}
