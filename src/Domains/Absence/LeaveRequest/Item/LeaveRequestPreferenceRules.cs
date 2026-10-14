namespace Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>A rule a leave request preference must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestPreferenceRule
{
    List<string> Check(LeaveRequestPreference item);
}

public sealed record LeaveRequestPreferenceRequiredCodeRule(string Message) : ILeaveRequestPreferenceRule
{
    public List<string> Check(LeaveRequestPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestPreferenceLeaveDaysRangeRule(int Min, int Max) : ILeaveRequestPreferenceRule
{
    public List<string> Check(LeaveRequestPreference item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestPreferenceRequiredNameRule(string Message) : ILeaveRequestPreferenceRule
{
    public List<string> Check(LeaveRequestPreference item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestPreferenceCodeLengthRule(int MaxLength) : ILeaveRequestPreferenceRule
{
    public List<string> Check(LeaveRequestPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestPreferenceCreatedAtRangeRule(int Min, int Max) : ILeaveRequestPreferenceRule
{
    public List<string> Check(LeaveRequestPreference item)
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

public sealed record LeaveRequestPreferenceRequiredSkillRule(string Message) : ILeaveRequestPreferenceRule
{
    public List<string> Check(LeaveRequestPreference item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request preference.</summary>
public sealed record LeaveRequestPreferenceValidator(List<ILeaveRequestPreferenceRule> Rules)
{
    public List<string> Validate(LeaveRequestPreference item)
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

    public void RequireValid(LeaveRequestPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestPreference> items)
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

/// <summary>Functions of leave request preference rules.</summary>
public static class LeaveRequestPreferenceRules
{
    public static LeaveRequestPreferenceValidator DefaultLeaveRequestPreferenceValidator()
    {
        return new LeaveRequestPreferenceValidator(
            new List<ILeaveRequestPreferenceRule>
            {
                new LeaveRequestPreferenceRequiredCodeRule("leave request preference needs a value"),
                new LeaveRequestPreferenceLeaveDaysRangeRule(80, 80),
                new LeaveRequestPreferenceRequiredNameRule("leave request preference needs this field"),
                new LeaveRequestPreferenceCodeLengthRule(255),
                new LeaveRequestPreferenceCreatedAtRangeRule(64, 86400),
                new LeaveRequestPreferenceRequiredSkillRule("leave request preference needs an entry"),
            });
    }
}
