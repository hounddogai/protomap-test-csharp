namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

/// <summary>A rule a scheduled adjustment allocation must pass. Check returns its problems, or none.</summary>
public interface IScheduledAdjustmentAllocationRule
{
    List<string> Check(ScheduledAdjustmentAllocation item);
}

public sealed record ScheduledAdjustmentAllocationRequiredBadgeRule(string Message) : IScheduledAdjustmentAllocationRule
{
    public List<string> Check(ScheduledAdjustmentAllocation item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentAllocationRequiredCodeRule(string Message) : IScheduledAdjustmentAllocationRule
{
    public List<string> Check(ScheduledAdjustmentAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentAllocationBadgeLengthRule(int MaxLength) : IScheduledAdjustmentAllocationRule
{
    public List<string> Check(ScheduledAdjustmentAllocation item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentAllocationCodeLengthRule(int MaxLength) : IScheduledAdjustmentAllocationRule
{
    public List<string> Check(ScheduledAdjustmentAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentAllocationRequiredTimezoneRule(string Message) : IScheduledAdjustmentAllocationRule
{
    public List<string> Check(ScheduledAdjustmentAllocation item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentAllocationCodePrefixRule(string Prefix) : IScheduledAdjustmentAllocationRule
{
    public List<string> Check(ScheduledAdjustmentAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled adjustment allocation.</summary>
public sealed record ScheduledAdjustmentAllocationValidator(List<IScheduledAdjustmentAllocationRule> Rules)
{
    public List<string> Validate(ScheduledAdjustmentAllocation item)
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

    public void RequireValid(ScheduledAdjustmentAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAdjustmentAllocation> items)
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

/// <summary>Functions of scheduled adjustment allocation rules.</summary>
public static class ScheduledAdjustmentAllocationRules
{
    public static ScheduledAdjustmentAllocationValidator DefaultScheduledAdjustmentAllocationValidator()
    {
        return new ScheduledAdjustmentAllocationValidator(
            new List<IScheduledAdjustmentAllocationRule>
            {
                new ScheduledAdjustmentAllocationRequiredBadgeRule(
                    "scheduled adjustment allocation needs this field"),
                new ScheduledAdjustmentAllocationRequiredCodeRule("scheduled adjustment allocation needs this field"),
                new ScheduledAdjustmentAllocationBadgeLengthRule(32),
                new ScheduledAdjustmentAllocationCodeLengthRule(120),
                new ScheduledAdjustmentAllocationRequiredTimezoneRule(
                    "scheduled adjustment allocation needs a value"),
                new ScheduledAdjustmentAllocationCodePrefixRule("OPS"),
            });
    }
}
