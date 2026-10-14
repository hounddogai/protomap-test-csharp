namespace Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>A rule a rolling mobility allocation must pass. Check returns its problems, or none.</summary>
public interface IRollingMobilityAllocationRule
{
    List<string> Check(RollingMobilityAllocation item);
}

public sealed record RollingMobilityAllocationMinimumRangeRule(double Min, double Max) : IRollingMobilityAllocationRule
{
    public List<string> Check(RollingMobilityAllocation item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityAllocationSequenceRangeRule(int Min, int Max) : IRollingMobilityAllocationRule
{
    public List<string> Check(RollingMobilityAllocation item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityAllocationCodePrefixRule(string Prefix) : IRollingMobilityAllocationRule
{
    public List<string> Check(RollingMobilityAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityAllocationVisibleGuardRule(int Limit) : IRollingMobilityAllocationRule
{
    public List<string> Check(RollingMobilityAllocation item)
    {
        if (item.Visible && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityAllocationStartsAtRangeRule(int Min, int Max) : IRollingMobilityAllocationRule
{
    public List<string> Check(RollingMobilityAllocation item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling mobility allocation.</summary>
public sealed record RollingMobilityAllocationValidator(List<IRollingMobilityAllocationRule> Rules)
{
    public List<string> Validate(RollingMobilityAllocation item)
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

    public void RequireValid(RollingMobilityAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingMobilityAllocation> items)
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

/// <summary>Functions of rolling mobility allocation rules.</summary>
public static class RollingMobilityAllocationRules
{
    public static RollingMobilityAllocationValidator DefaultRollingMobilityAllocationValidator()
    {
        return new RollingMobilityAllocationValidator(
            new List<IRollingMobilityAllocationRule>
            {
                new RollingMobilityAllocationMinimumRangeRule(8.2, 1754.6),
                new RollingMobilityAllocationSequenceRangeRule(255, 86400),
                new RollingMobilityAllocationCodePrefixRule("X"),
                new RollingMobilityAllocationVisibleGuardRule(120),
                new RollingMobilityAllocationStartsAtRangeRule(32, 80),
            });
    }
}
