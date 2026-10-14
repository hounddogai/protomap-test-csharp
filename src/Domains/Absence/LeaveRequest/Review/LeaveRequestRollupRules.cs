namespace Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>A rule a leave request rollup must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestRollupRule
{
    List<string> Check(LeaveRequestRollup item);
}

public sealed record LeaveRequestRollupJobCodeLengthRule(int MaxLength) : ILeaveRequestRollupRule
{
    public List<string> Check(LeaveRequestRollup item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestRollupCodePrefixRule(string Prefix) : ILeaveRequestRollupRule
{
    public List<string> Check(LeaveRequestRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestRollupCodeLengthRule(int MaxLength) : ILeaveRequestRollupRule
{
    public List<string> Check(LeaveRequestRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestRollupVersionRangeRule(int Min, int Max) : ILeaveRequestRollupRule
{
    public List<string> Check(LeaveRequestRollup item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestRollupRequiredJobCodeRule(string Message) : ILeaveRequestRollupRule
{
    public List<string> Check(LeaveRequestRollup item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestRollupFteRangeRule(double Min, double Max) : ILeaveRequestRollupRule
{
    public List<string> Check(LeaveRequestRollup item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request rollup.</summary>
public sealed record LeaveRequestRollupValidator(List<ILeaveRequestRollupRule> Rules)
{
    public List<string> Validate(LeaveRequestRollup item)
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

    public void RequireValid(LeaveRequestRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestRollup> items)
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

/// <summary>Functions of leave request rollup rules.</summary>
public static class LeaveRequestRollupRules
{
    public static LeaveRequestRollupValidator DefaultLeaveRequestRollupValidator()
    {
        return new LeaveRequestRollupValidator(
            new List<ILeaveRequestRollupRule>
            {
                new LeaveRequestRollupJobCodeLengthRule(3600),
                new LeaveRequestRollupCodePrefixRule("X"),
                new LeaveRequestRollupCodeLengthRule(1000),
                new LeaveRequestRollupVersionRangeRule(32, 64),
                new LeaveRequestRollupRequiredJobCodeRule("leave request rollup needs an entry"),
                new LeaveRequestRollupFteRangeRule(0.9, 993.8),
            });
    }
}
