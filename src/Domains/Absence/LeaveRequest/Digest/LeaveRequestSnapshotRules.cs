namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request snapshot must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestSnapshotRule
{
    List<string> Check(LeaveRequestSnapshot item);
}

public sealed record LeaveRequestSnapshotLabelLengthRule(int MaxLength) : ILeaveRequestSnapshotRule
{
    public List<string> Check(LeaveRequestSnapshot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestSnapshotWindowDaysRangeRule(int Min, int Max) : ILeaveRequestSnapshotRule
{
    public List<string> Check(LeaveRequestSnapshot item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LeaveRequestSnapshotCodePrefixRule(string Prefix) : ILeaveRequestSnapshotRule
{
    public List<string> Check(LeaveRequestSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestSnapshotAllowedCodeRule(List<string> Allowed) : ILeaveRequestSnapshotRule
{
    public List<string> Check(LeaveRequestSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestSnapshotChannelLengthRule(int MaxLength) : ILeaveRequestSnapshotRule
{
    public List<string> Check(LeaveRequestSnapshot item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestSnapshotRequiredLabelRule(string Message) : ILeaveRequestSnapshotRule
{
    public List<string> Check(LeaveRequestSnapshot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request snapshot.</summary>
public sealed record LeaveRequestSnapshotValidator(List<ILeaveRequestSnapshotRule> Rules)
{
    public List<string> Validate(LeaveRequestSnapshot item)
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

    public void RequireValid(LeaveRequestSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestSnapshot> items)
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

/// <summary>Functions of leave request snapshot rules.</summary>
public static class LeaveRequestSnapshotRules
{
    public static LeaveRequestSnapshotValidator DefaultLeaveRequestSnapshotValidator()
    {
        return new LeaveRequestSnapshotValidator(
            new List<ILeaveRequestSnapshotRule>
            {
                new LeaveRequestSnapshotLabelLengthRule(80),
                new LeaveRequestSnapshotWindowDaysRangeRule(16, 3600),
                new LeaveRequestSnapshotCodePrefixRule("X"),
                new LeaveRequestSnapshotAllowedCodeRule(new List<string> { "seasonal", "scheduled", "strict" }),
                new LeaveRequestSnapshotChannelLengthRule(3600),
                new LeaveRequestSnapshotRequiredLabelRule("leave request snapshot needs a value"),
            });
    }
}
