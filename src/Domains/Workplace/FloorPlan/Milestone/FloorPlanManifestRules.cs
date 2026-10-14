namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan manifest must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanManifestRule
{
    List<string> Check(FloorPlanManifest item);
}

public sealed record FloorPlanManifestCodePrefixRule(string Prefix) : IFloorPlanManifestRule
{
    public List<string> Check(FloorPlanManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanManifestLimitRangeRule(int Min, int Max) : IFloorPlanManifestRule
{
    public List<string> Check(FloorPlanManifest item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FloorPlanManifestAllowedRegionRule(List<string> Allowed) : IFloorPlanManifestRule
{
    public List<string> Check(FloorPlanManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanManifestCreatedAtRangeRule(int Min, int Max) : IFloorPlanManifestRule
{
    public List<string> Check(FloorPlanManifest item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a floor plan manifest.</summary>
public sealed record FloorPlanManifestValidator(List<IFloorPlanManifestRule> Rules)
{
    public List<string> Validate(FloorPlanManifest item)
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

    public void RequireValid(FloorPlanManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanManifest> items)
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

/// <summary>Functions of floor plan manifest rules.</summary>
public static class FloorPlanManifestRules
{
    public static FloorPlanManifestValidator DefaultFloorPlanManifestValidator()
    {
        return new FloorPlanManifestValidator(
            new List<IFloorPlanManifestRule>
            {
                new FloorPlanManifestCodePrefixRule("REF"),
                new FloorPlanManifestLimitRangeRule(80, 80),
                new FloorPlanManifestAllowedRegionRule(new List<string> { "scheduled" }),
                new FloorPlanManifestCreatedAtRangeRule(120, 1000),
            });
    }
}
