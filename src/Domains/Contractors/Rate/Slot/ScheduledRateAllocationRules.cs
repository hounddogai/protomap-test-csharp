namespace Acme.Workforce.Contractors.Rate.Slot;

/// <summary>A rule a scheduled rate allocation must pass. Check returns its problems, or none.</summary>
public interface IScheduledRateAllocationRule
{
    List<string> Check(ScheduledRateAllocation item);
}

public sealed record ScheduledRateAllocationExpiresAtRangeRule(int Min, int Max) : IScheduledRateAllocationRule
{
    public List<string> Check(ScheduledRateAllocation item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRateAllocationRequiredCodeRule(string Message) : IScheduledRateAllocationRule
{
    public List<string> Check(ScheduledRateAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRateAllocationPriorityRangeRule(int Min, int Max) : IScheduledRateAllocationRule
{
    public List<string> Check(ScheduledRateAllocation item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledRateAllocationDepartmentLengthRule(int MaxLength) : IScheduledRateAllocationRule
{
    public List<string> Check(ScheduledRateAllocation item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRateAllocationCodePrefixRule(string Prefix) : IScheduledRateAllocationRule
{
    public List<string> Check(ScheduledRateAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRateAllocationCodeLengthRule(int MaxLength) : IScheduledRateAllocationRule
{
    public List<string> Check(ScheduledRateAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled rate allocation.</summary>
public sealed record ScheduledRateAllocationValidator(List<IScheduledRateAllocationRule> Rules)
{
    public List<string> Validate(ScheduledRateAllocation item)
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

    public void RequireValid(ScheduledRateAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledRateAllocation> items)
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

/// <summary>Functions of scheduled rate allocation rules.</summary>
public static class ScheduledRateAllocationRules
{
    public static ScheduledRateAllocationValidator DefaultScheduledRateAllocationValidator()
    {
        return new ScheduledRateAllocationValidator(
            new List<IScheduledRateAllocationRule>
            {
                new ScheduledRateAllocationExpiresAtRangeRule(32, 32),
                new ScheduledRateAllocationRequiredCodeRule("scheduled rate allocation needs an entry"),
                new ScheduledRateAllocationPriorityRangeRule(32, 86400),
                new ScheduledRateAllocationDepartmentLengthRule(86400),
                new ScheduledRateAllocationCodePrefixRule("REF"),
                new ScheduledRateAllocationCodeLengthRule(255),
            });
    }
}
