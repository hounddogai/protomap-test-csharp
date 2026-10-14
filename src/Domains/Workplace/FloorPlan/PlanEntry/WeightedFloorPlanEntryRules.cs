namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted floor plan entry must pass. Check returns its problems, or none.</summary>
public interface IWeightedFloorPlanEntryRule
{
    List<string> Check(WeightedFloorPlanEntry item);
}

public sealed record WeightedFloorPlanEntryCodePrefixRule(string Prefix) : IWeightedFloorPlanEntryRule
{
    public List<string> Check(WeightedFloorPlanEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedFloorPlanEntryMinimumRangeRule(double Min, double Max) : IWeightedFloorPlanEntryRule
{
    public List<string> Check(WeightedFloorPlanEntry item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedFloorPlanEntryRequiredLabelRule(string Message) : IWeightedFloorPlanEntryRule
{
    public List<string> Check(WeightedFloorPlanEntry item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedFloorPlanEntryAllowedLabelRule(List<string> Allowed) : IWeightedFloorPlanEntryRule
{
    public List<string> Check(WeightedFloorPlanEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record WeightedFloorPlanEntryBudgetRangeRule(double Min, double Max) : IWeightedFloorPlanEntryRule
{
    public List<string> Check(WeightedFloorPlanEntry item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedFloorPlanEntryExpiresAtRangeRule(int Min, int Max) : IWeightedFloorPlanEntryRule
{
    public List<string> Check(WeightedFloorPlanEntry item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted floor plan entry.</summary>
public sealed record WeightedFloorPlanEntryValidator(List<IWeightedFloorPlanEntryRule> Rules)
{
    public List<string> Validate(WeightedFloorPlanEntry item)
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

    public void RequireValid(WeightedFloorPlanEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedFloorPlanEntry> items)
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

/// <summary>Functions of weighted floor plan entry rules.</summary>
public static class WeightedFloorPlanEntryRules
{
    public static WeightedFloorPlanEntryValidator DefaultWeightedFloorPlanEntryValidator()
    {
        return new WeightedFloorPlanEntryValidator(
            new List<IWeightedFloorPlanEntryRule>
            {
                new WeightedFloorPlanEntryCodePrefixRule("X"),
                new WeightedFloorPlanEntryMinimumRangeRule(4.7, 501.3),
                new WeightedFloorPlanEntryRequiredLabelRule("weighted floor plan entry needs a value"),
                new WeightedFloorPlanEntryAllowedLabelRule(new List<string> { "batched" }),
                new WeightedFloorPlanEntryBudgetRangeRule(0.3, 2329.6),
                new WeightedFloorPlanEntryExpiresAtRangeRule(64, 80),
            });
    }
}
