namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan policy must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanPolicyRule
{
    List<string> Check(FloorPlanPolicy item);
}

public sealed record FloorPlanPolicyAllowedDescriptionRule(List<string> Allowed) : IFloorPlanPolicyRule
{
    public List<string> Check(FloorPlanPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanPolicyCodePrefixRule(string Prefix) : IFloorPlanPolicyRule
{
    public List<string> Check(FloorPlanPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanPolicyRequiredTitleRule(string Message) : IFloorPlanPolicyRule
{
    public List<string> Check(FloorPlanPolicy item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanPolicyVisibleGuardRule(int Limit) : IFloorPlanPolicyRule
{
    public List<string> Check(FloorPlanPolicy item)
    {
        if (item.Visible && item.WindowDays > Limit)
        {
            return new List<string> { $"window days {item.WindowDays} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanPolicyRequiredDescriptionRule(string Message) : IFloorPlanPolicyRule
{
    public List<string> Check(FloorPlanPolicy item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanPolicyBudgetRangeRule(double Min, double Max) : IFloorPlanPolicyRule
{
    public List<string> Check(FloorPlanPolicy item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan policy.</summary>
public sealed record FloorPlanPolicyValidator(List<IFloorPlanPolicyRule> Rules)
{
    public List<string> Validate(FloorPlanPolicy item)
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

    public void RequireValid(FloorPlanPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanPolicy> items)
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

/// <summary>Functions of floor plan policy rules.</summary>
public static class FloorPlanPolicyRules
{
    public static FloorPlanPolicyValidator DefaultFloorPlanPolicyValidator()
    {
        return new FloorPlanPolicyValidator(
            new List<IFloorPlanPolicyRule>
            {
                new FloorPlanPolicyAllowedDescriptionRule(new List<string> { "regional", "primary", "primary" }),
                new FloorPlanPolicyCodePrefixRule("X"),
                new FloorPlanPolicyRequiredTitleRule("floor plan policy needs this field"),
                new FloorPlanPolicyVisibleGuardRule(1000),
                new FloorPlanPolicyRequiredDescriptionRule("floor plan policy needs this field"),
                new FloorPlanPolicyBudgetRangeRule(5.7, 92.6),
            });
    }
}
