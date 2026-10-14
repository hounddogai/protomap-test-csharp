namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a leave request score must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestScoreRule
{
    List<string> Check(LeaveRequestScore item);
}

public sealed record LeaveRequestScoreAllowedEmployeeIdRule(List<string> Allowed) : ILeaveRequestScoreRule
{
    public List<string> Check(LeaveRequestScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestScoreCostRangeRule(double Min, double Max) : ILeaveRequestScoreRule
{
    public List<string> Check(LeaveRequestScore item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LeaveRequestScoreRequiredCodeRule(string Message) : ILeaveRequestScoreRule
{
    public List<string> Check(LeaveRequestScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestScoreEmployeeIdLengthRule(int MaxLength) : ILeaveRequestScoreRule
{
    public List<string> Check(LeaveRequestScore item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestScoreCodePrefixRule(string Prefix) : ILeaveRequestScoreRule
{
    public List<string> Check(LeaveRequestScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request score.</summary>
public sealed record LeaveRequestScoreValidator(List<ILeaveRequestScoreRule> Rules)
{
    public List<string> Validate(LeaveRequestScore item)
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

    public void RequireValid(LeaveRequestScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestScore> items)
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

/// <summary>Functions of leave request score rules.</summary>
public static class LeaveRequestScoreRules
{
    public static LeaveRequestScoreValidator DefaultLeaveRequestScoreValidator()
    {
        return new LeaveRequestScoreValidator(
            new List<ILeaveRequestScoreRule>
            {
                new LeaveRequestScoreAllowedEmployeeIdRule(new List<string> { "rolling", "scheduled", "capped" }),
                new LeaveRequestScoreCostRangeRule(6.8, 3836.3),
                new LeaveRequestScoreRequiredCodeRule("leave request score needs a value"),
                new LeaveRequestScoreEmployeeIdLengthRule(64),
                new LeaveRequestScoreCodePrefixRule("OPS"),
            });
    }
}
