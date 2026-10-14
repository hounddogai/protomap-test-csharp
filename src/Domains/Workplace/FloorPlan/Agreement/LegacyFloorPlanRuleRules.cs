namespace Acme.Workforce.Workplace.FloorPlan.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy floor plan rule must pass. Check returns its problems, or none.</summary>
public interface ILegacyFloorPlanRuleRule
{
    List<string> Check(LegacyFloorPlanRule item);
}

public sealed record LegacyFloorPlanRuleAllowedLabelRule(List<string> Allowed) : ILegacyFloorPlanRuleRule
{
    public List<string> Check(LegacyFloorPlanRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record LegacyFloorPlanRuleRequiredSkillRule(string Message) : ILegacyFloorPlanRuleRule
{
    public List<string> Check(LegacyFloorPlanRule item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyFloorPlanRuleWeightRangeRule(double Min, double Max) : ILegacyFloorPlanRuleRule
{
    public List<string> Check(LegacyFloorPlanRule item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy floor plan rule.</summary>
public sealed record LegacyFloorPlanRuleValidator(List<ILegacyFloorPlanRuleRule> Rules)
{
    public List<string> Validate(LegacyFloorPlanRule item)
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

    public void RequireValid(LegacyFloorPlanRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyFloorPlanRule> items)
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

/// <summary>Functions of legacy floor plan rule rules.</summary>
public static class LegacyFloorPlanRuleRules
{
    public static LegacyFloorPlanRuleValidator DefaultLegacyFloorPlanRuleValidator()
    {
        return new LegacyFloorPlanRuleValidator(
            new List<ILegacyFloorPlanRuleRule>
            {
                new LegacyFloorPlanRuleAllowedLabelRule(new List<string> { "capped", "regional" }),
                new LegacyFloorPlanRuleRequiredSkillRule("legacy floor plan rule needs an entry"),
                new LegacyFloorPlanRuleWeightRangeRule(2.9, 4419.1),
            });
    }
}
