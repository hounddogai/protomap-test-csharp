namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>A rule a legacy floor plan threshold must pass. Check returns its problems, or none.</summary>
public interface ILegacyFloorPlanThresholdRule
{
    List<string> Check(LegacyFloorPlanThreshold item);
}

public sealed record LegacyFloorPlanThresholdCodeLengthRule(int MaxLength) : ILegacyFloorPlanThresholdRule
{
    public List<string> Check(LegacyFloorPlanThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyFloorPlanThresholdPinnedGuardRule(double Limit) : ILegacyFloorPlanThresholdRule
{
    public List<string> Check(LegacyFloorPlanThreshold item)
    {
        if (item.Pinned && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record LegacyFloorPlanThresholdTitleLengthRule(int MaxLength) : ILegacyFloorPlanThresholdRule
{
    public List<string> Check(LegacyFloorPlanThreshold item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyFloorPlanThresholdRequiredTitleRule(string Message) : ILegacyFloorPlanThresholdRule
{
    public List<string> Check(LegacyFloorPlanThreshold item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyFloorPlanThresholdRequiredCodeRule(string Message) : ILegacyFloorPlanThresholdRule
{
    public List<string> Check(LegacyFloorPlanThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyFloorPlanThresholdCodePrefixRule(string Prefix) : ILegacyFloorPlanThresholdRule
{
    public List<string> Check(LegacyFloorPlanThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy floor plan threshold.</summary>
public sealed record LegacyFloorPlanThresholdValidator(List<ILegacyFloorPlanThresholdRule> Rules)
{
    public List<string> Validate(LegacyFloorPlanThreshold item)
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

    public void RequireValid(LegacyFloorPlanThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyFloorPlanThreshold> items)
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

/// <summary>Functions of legacy floor plan threshold rules.</summary>
public static class LegacyFloorPlanThresholdRules
{
    public static LegacyFloorPlanThresholdValidator DefaultLegacyFloorPlanThresholdValidator()
    {
        return new LegacyFloorPlanThresholdValidator(
            new List<ILegacyFloorPlanThresholdRule>
            {
                new LegacyFloorPlanThresholdCodeLengthRule(16),
                new LegacyFloorPlanThresholdPinnedGuardRule(2.0),
                new LegacyFloorPlanThresholdTitleLengthRule(86400),
                new LegacyFloorPlanThresholdRequiredTitleRule("legacy floor plan threshold needs an entry"),
                new LegacyFloorPlanThresholdRequiredCodeRule("legacy floor plan threshold needs an entry"),
                new LegacyFloorPlanThresholdCodePrefixRule("ACM"),
            });
    }
}
