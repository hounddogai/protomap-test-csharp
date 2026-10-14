namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request window must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestWindowRule
{
    List<string> Check(LeaveRequestWindow item);
}

public sealed record LeaveRequestWindowDepartmentLengthRule(int MaxLength) : ILeaveRequestWindowRule
{
    public List<string> Check(LeaveRequestWindow item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestWindowAllowedNameRule(List<string> Allowed) : ILeaveRequestWindowRule
{
    public List<string> Check(LeaveRequestWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestWindowNameLengthRule(int MaxLength) : ILeaveRequestWindowRule
{
    public List<string> Check(LeaveRequestWindow item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestWindowCodePrefixRule(string Prefix) : ILeaveRequestWindowRule
{
    public List<string> Check(LeaveRequestWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestWindowRequiredCodeRule(string Message) : ILeaveRequestWindowRule
{
    public List<string> Check(LeaveRequestWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestWindowRevisionRangeRule(int Min, int Max) : ILeaveRequestWindowRule
{
    public List<string> Check(LeaveRequestWindow item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a leave request window.</summary>
public sealed record LeaveRequestWindowValidator(List<ILeaveRequestWindowRule> Rules)
{
    public List<string> Validate(LeaveRequestWindow item)
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

    public void RequireValid(LeaveRequestWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestWindow> items)
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

/// <summary>Functions of leave request window rules.</summary>
public static class LeaveRequestWindowRules
{
    public static LeaveRequestWindowValidator DefaultLeaveRequestWindowValidator()
    {
        return new LeaveRequestWindowValidator(
            new List<ILeaveRequestWindowRule>
            {
                new LeaveRequestWindowDepartmentLengthRule(120),
                new LeaveRequestWindowAllowedNameRule(new List<string> { "pending", "regional" }),
                new LeaveRequestWindowNameLengthRule(255),
                new LeaveRequestWindowCodePrefixRule("Q"),
                new LeaveRequestWindowRequiredCodeRule("leave request window needs an entry"),
                new LeaveRequestWindowRevisionRangeRule(64, 80),
            });
    }
}
