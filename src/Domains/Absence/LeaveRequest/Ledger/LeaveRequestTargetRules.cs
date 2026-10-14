namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>A rule a leave request target must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestTargetRule
{
    List<string> Check(LeaveRequestTarget item);
}

public sealed record LeaveRequestTargetRequiredLabelRule(string Message) : ILeaveRequestTargetRule
{
    public List<string> Check(LeaveRequestTarget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestTargetSegmentLengthRule(int MaxLength) : ILeaveRequestTargetRule
{
    public List<string> Check(LeaveRequestTarget item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestTargetRequiredSegmentRule(string Message) : ILeaveRequestTargetRule
{
    public List<string> Check(LeaveRequestTarget item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestTargetCodePrefixRule(string Prefix) : ILeaveRequestTargetRule
{
    public List<string> Check(LeaveRequestTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request target.</summary>
public sealed record LeaveRequestTargetValidator(List<ILeaveRequestTargetRule> Rules)
{
    public List<string> Validate(LeaveRequestTarget item)
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

    public void RequireValid(LeaveRequestTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestTarget> items)
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

/// <summary>Functions of leave request target rules.</summary>
public static class LeaveRequestTargetRules
{
    public static LeaveRequestTargetValidator DefaultLeaveRequestTargetValidator()
    {
        return new LeaveRequestTargetValidator(
            new List<ILeaveRequestTargetRule>
            {
                new LeaveRequestTargetRequiredLabelRule("leave request target needs a value"),
                new LeaveRequestTargetSegmentLengthRule(64),
                new LeaveRequestTargetRequiredSegmentRule("leave request target needs an entry"),
                new LeaveRequestTargetCodePrefixRule("OPS"),
            });
    }
}
