namespace Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>A rule a leave request item must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestItemRule
{
    List<string> Check(LeaveRequestItem item);
}

public sealed record LeaveRequestItemLeaveDaysRangeRule(int Min, int Max) : ILeaveRequestItemRule
{
    public List<string> Check(LeaveRequestItem item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LeaveRequestItemRevisionRangeRule(int Min, int Max) : ILeaveRequestItemRule
{
    public List<string> Check(LeaveRequestItem item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestItemDescriptionLengthRule(int MaxLength) : ILeaveRequestItemRule
{
    public List<string> Check(LeaveRequestItem item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestItemRequiredTitleRule(string Message) : ILeaveRequestItemRule
{
    public List<string> Check(LeaveRequestItem item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request item.</summary>
public sealed record LeaveRequestItemValidator(List<ILeaveRequestItemRule> Rules)
{
    public List<string> Validate(LeaveRequestItem item)
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

    public void RequireValid(LeaveRequestItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestItem> items)
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

/// <summary>Functions of leave request item rules.</summary>
public static class LeaveRequestItemRules
{
    public static LeaveRequestItemValidator DefaultLeaveRequestItemValidator()
    {
        return new LeaveRequestItemValidator(
            new List<ILeaveRequestItemRule>
            {
                new LeaveRequestItemLeaveDaysRangeRule(120, 86400),
                new LeaveRequestItemRevisionRangeRule(32, 86400),
                new LeaveRequestItemDescriptionLengthRule(32),
                new LeaveRequestItemRequiredTitleRule("leave request item needs a value"),
            });
    }
}
