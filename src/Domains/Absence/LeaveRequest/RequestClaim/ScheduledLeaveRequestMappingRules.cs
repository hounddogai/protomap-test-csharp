namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>A rule a scheduled leave request mapping must pass. Check returns its problems, or none.</summary>
public interface IScheduledLeaveRequestMappingRule
{
    List<string> Check(ScheduledLeaveRequestMapping item);
}

public sealed record ScheduledLeaveRequestMappingCodePrefixRule(string Prefix) : IScheduledLeaveRequestMappingRule
{
    public List<string> Check(ScheduledLeaveRequestMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestMappingSegmentLengthRule(int MaxLength) : IScheduledLeaveRequestMappingRule
{
    public List<string> Check(ScheduledLeaveRequestMapping item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestMappingRequiredSegmentRule(string Message) : IScheduledLeaveRequestMappingRule
{
    public List<string> Check(ScheduledLeaveRequestMapping item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestMappingCodeLengthRule(int MaxLength) : IScheduledLeaveRequestMappingRule
{
    public List<string> Check(ScheduledLeaveRequestMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestMappingRequiredTitleRule(string Message) : IScheduledLeaveRequestMappingRule
{
    public List<string> Check(ScheduledLeaveRequestMapping item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled leave request mapping.</summary>
public sealed record ScheduledLeaveRequestMappingValidator(List<IScheduledLeaveRequestMappingRule> Rules)
{
    public List<string> Validate(ScheduledLeaveRequestMapping item)
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

    public void RequireValid(ScheduledLeaveRequestMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledLeaveRequestMapping> items)
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

/// <summary>Functions of scheduled leave request mapping rules.</summary>
public static class ScheduledLeaveRequestMappingRules
{
    public static ScheduledLeaveRequestMappingValidator DefaultScheduledLeaveRequestMappingValidator()
    {
        return new ScheduledLeaveRequestMappingValidator(
            new List<IScheduledLeaveRequestMappingRule>
            {
                new ScheduledLeaveRequestMappingCodePrefixRule("ACM"),
                new ScheduledLeaveRequestMappingSegmentLengthRule(16),
                new ScheduledLeaveRequestMappingRequiredSegmentRule("scheduled leave request mapping needs a value"),
                new ScheduledLeaveRequestMappingCodeLengthRule(86400),
                new ScheduledLeaveRequestMappingRequiredTitleRule("scheduled leave request mapping needs this field"),
            });
    }
}
