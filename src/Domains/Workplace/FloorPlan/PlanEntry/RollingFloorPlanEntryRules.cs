namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling floor plan entry must pass. Check returns its problems, or none.</summary>
public interface IRollingFloorPlanEntryRule
{
    List<string> Check(RollingFloorPlanEntry item);
}

public sealed record RollingFloorPlanEntryCodePrefixRule(string Prefix) : IRollingFloorPlanEntryRule
{
    public List<string> Check(RollingFloorPlanEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanEntryAllowedCountryRule(List<string> Allowed) : IRollingFloorPlanEntryRule
{
    public List<string> Check(RollingFloorPlanEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record RollingFloorPlanEntryMaximumRangeRule(double Min, double Max) : IRollingFloorPlanEntryRule
{
    public List<string> Check(RollingFloorPlanEntry item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling floor plan entry.</summary>
public sealed record RollingFloorPlanEntryValidator(List<IRollingFloorPlanEntryRule> Rules)
{
    public List<string> Validate(RollingFloorPlanEntry item)
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

    public void RequireValid(RollingFloorPlanEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingFloorPlanEntry> items)
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

/// <summary>Functions of rolling floor plan entry rules.</summary>
public static class RollingFloorPlanEntryRules
{
    public static RollingFloorPlanEntryValidator DefaultRollingFloorPlanEntryValidator()
    {
        return new RollingFloorPlanEntryValidator(
            new List<IRollingFloorPlanEntryRule>
            {
                new RollingFloorPlanEntryCodePrefixRule("Q"),
                new RollingFloorPlanEntryAllowedCountryRule(new List<string> { "global" }),
                new RollingFloorPlanEntryMaximumRangeRule(1.7, 125.6),
            });
    }
}
