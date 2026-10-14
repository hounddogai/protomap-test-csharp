namespace Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>A rule a scheduled leave request snapshot must pass. Check returns its problems, or none.</summary>
public interface IScheduledLeaveRequestSnapshotRule
{
    List<string> Check(ScheduledLeaveRequestSnapshot item);
}

public sealed record ScheduledLeaveRequestSnapshotCodeLengthRule(int MaxLength) : IScheduledLeaveRequestSnapshotRule
{
    public List<string> Check(ScheduledLeaveRequestSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestSnapshotRequiredTitleRule(string Message) : IScheduledLeaveRequestSnapshotRule
{
    public List<string> Check(ScheduledLeaveRequestSnapshot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestSnapshotRequiredCodeRule(string Message) : IScheduledLeaveRequestSnapshotRule
{
    public List<string> Check(ScheduledLeaveRequestSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestSnapshotCreatedAtRangeRule(int Min, int Max) : IScheduledLeaveRequestSnapshotRule
{
    public List<string> Check(ScheduledLeaveRequestSnapshot item)
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

public sealed record ScheduledLeaveRequestSnapshotPriorityRangeRule(int Min, int Max) : IScheduledLeaveRequestSnapshotRule
{
    public List<string> Check(ScheduledLeaveRequestSnapshot item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled leave request snapshot.</summary>
public sealed record ScheduledLeaveRequestSnapshotValidator(List<IScheduledLeaveRequestSnapshotRule> Rules)
{
    public List<string> Validate(ScheduledLeaveRequestSnapshot item)
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

    public void RequireValid(ScheduledLeaveRequestSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledLeaveRequestSnapshot> items)
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

/// <summary>Functions of scheduled leave request snapshot rules.</summary>
public static class ScheduledLeaveRequestSnapshotRules
{
    public static ScheduledLeaveRequestSnapshotValidator DefaultScheduledLeaveRequestSnapshotValidator()
    {
        return new ScheduledLeaveRequestSnapshotValidator(
            new List<IScheduledLeaveRequestSnapshotRule>
            {
                new ScheduledLeaveRequestSnapshotCodeLengthRule(16),
                new ScheduledLeaveRequestSnapshotRequiredTitleRule("scheduled leave request snapshot needs an entry"),
                new ScheduledLeaveRequestSnapshotRequiredCodeRule("scheduled leave request snapshot needs a value"),
                new ScheduledLeaveRequestSnapshotCreatedAtRangeRule(64, 3600),
                new ScheduledLeaveRequestSnapshotPriorityRangeRule(80, 80),
            });
    }
}
