namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>A rule a leave request notice must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestNoticeRule
{
    List<string> Check(LeaveRequestNotice item);
}

public sealed record LeaveRequestNoticeNameLengthRule(int MaxLength) : ILeaveRequestNoticeRule
{
    public List<string> Check(LeaveRequestNotice item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestNoticeWeightRangeRule(double Min, double Max) : ILeaveRequestNoticeRule
{
    public List<string> Check(LeaveRequestNotice item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestNoticeRequiredCodeRule(string Message) : ILeaveRequestNoticeRule
{
    public List<string> Check(LeaveRequestNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestNoticeLeaveDaysRangeRule(int Min, int Max) : ILeaveRequestNoticeRule
{
    public List<string> Check(LeaveRequestNotice item)
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

/// <summary>Runs every rule of a leave request notice.</summary>
public sealed record LeaveRequestNoticeValidator(List<ILeaveRequestNoticeRule> Rules)
{
    public List<string> Validate(LeaveRequestNotice item)
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

    public void RequireValid(LeaveRequestNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestNotice> items)
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

/// <summary>Functions of leave request notice rules.</summary>
public static class LeaveRequestNoticeRules
{
    public static LeaveRequestNoticeValidator DefaultLeaveRequestNoticeValidator()
    {
        return new LeaveRequestNoticeValidator(
            new List<ILeaveRequestNoticeRule>
            {
                new LeaveRequestNoticeNameLengthRule(120),
                new LeaveRequestNoticeWeightRangeRule(8.3, 1822.1),
                new LeaveRequestNoticeRequiredCodeRule("leave request notice needs an entry"),
                new LeaveRequestNoticeLeaveDaysRangeRule(64, 3600),
            });
    }
}
