namespace Acme.Workforce.Workplace.FloorPlan.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan rule must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanRuleRule
{
    List<string> Check(FloorPlanRule item);
}

public sealed record FloorPlanRuleCodeLengthRule(int MaxLength) : IFloorPlanRuleRule
{
    public List<string> Check(FloorPlanRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanRuleAllowedCodeRule(List<string> Allowed) : IFloorPlanRuleRule
{
    public List<string> Check(FloorPlanRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanRuleLabelLengthRule(int MaxLength) : IFloorPlanRuleRule
{
    public List<string> Check(FloorPlanRule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanRuleDiscountRangeRule(double Min, double Max) : IFloorPlanRuleRule
{
    public List<string> Check(FloorPlanRule item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FloorPlanRuleRequiredRegionRule(string Message) : IFloorPlanRuleRule
{
    public List<string> Check(FloorPlanRule item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan rule.</summary>
public sealed record FloorPlanRuleValidator(List<IFloorPlanRuleRule> Rules)
{
    public List<string> Validate(FloorPlanRule item)
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

    public void RequireValid(FloorPlanRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanRule> items)
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

/// <summary>Functions of floor plan rule rules.</summary>
public static class FloorPlanRuleRules
{
    public static FloorPlanRuleValidator DefaultFloorPlanRuleValidator()
    {
        return new FloorPlanRuleValidator(
            new List<IFloorPlanRuleRule>
            {
                new FloorPlanRuleCodeLengthRule(32),
                new FloorPlanRuleAllowedCodeRule(new List<string> { "batched", "seasonal" }),
                new FloorPlanRuleLabelLengthRule(64),
                new FloorPlanRuleDiscountRangeRule(7.9, 2411.6),
                new FloorPlanRuleRequiredRegionRule("floor plan rule needs this field"),
            });
    }
}
