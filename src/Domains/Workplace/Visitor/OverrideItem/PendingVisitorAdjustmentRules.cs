namespace Acme.Workforce.Workplace.Visitor.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending visitor adjustment must pass. Check returns its problems, or none.</summary>
public interface IPendingVisitorAdjustmentRule
{
    List<string> Check(PendingVisitorAdjustment item);
}

public sealed record PendingVisitorAdjustmentRequiredCodeRule(string Message) : IPendingVisitorAdjustmentRule
{
    public List<string> Check(PendingVisitorAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingVisitorAdjustmentCreatedAtRangeRule(int Min, int Max) : IPendingVisitorAdjustmentRule
{
    public List<string> Check(PendingVisitorAdjustment item)
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

public sealed record PendingVisitorAdjustmentCodePrefixRule(string Prefix) : IPendingVisitorAdjustmentRule
{
    public List<string> Check(PendingVisitorAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingVisitorAdjustmentAllowedDescriptionRule(List<string> Allowed) : IPendingVisitorAdjustmentRule
{
    public List<string> Check(PendingVisitorAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PendingVisitorAdjustmentRequiredRegionRule(string Message) : IPendingVisitorAdjustmentRule
{
    public List<string> Check(PendingVisitorAdjustment item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingVisitorAdjustmentVersionRangeRule(int Min, int Max) : IPendingVisitorAdjustmentRule
{
    public List<string> Check(PendingVisitorAdjustment item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending visitor adjustment.</summary>
public sealed record PendingVisitorAdjustmentValidator(List<IPendingVisitorAdjustmentRule> Rules)
{
    public List<string> Validate(PendingVisitorAdjustment item)
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

    public void RequireValid(PendingVisitorAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingVisitorAdjustment> items)
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

/// <summary>Functions of pending visitor adjustment rules.</summary>
public static class PendingVisitorAdjustmentRules
{
    public static PendingVisitorAdjustmentValidator DefaultPendingVisitorAdjustmentValidator()
    {
        return new PendingVisitorAdjustmentValidator(
            new List<IPendingVisitorAdjustmentRule>
            {
                new PendingVisitorAdjustmentRequiredCodeRule("pending visitor adjustment needs a value"),
                new PendingVisitorAdjustmentCreatedAtRangeRule(120, 1000),
                new PendingVisitorAdjustmentCodePrefixRule("ACM"),
                new PendingVisitorAdjustmentAllowedDescriptionRule(
                    new List<string> { "scheduled", "fallback", "global" }),
                new PendingVisitorAdjustmentRequiredRegionRule("pending visitor adjustment needs this field"),
                new PendingVisitorAdjustmentVersionRangeRule(32, 3600),
            });
    }
}
