namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

/// <summary>A rule a leave request exception must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestExceptionRule
{
    List<string> Check(LeaveRequestException item);
}

public sealed record LeaveRequestExceptionLabelLengthRule(int MaxLength) : ILeaveRequestExceptionRule
{
    public List<string> Check(LeaveRequestException item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestExceptionRequiredLabelRule(string Message) : ILeaveRequestExceptionRule
{
    public List<string> Check(LeaveRequestException item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestExceptionMaximumRangeRule(double Min, double Max) : ILeaveRequestExceptionRule
{
    public List<string> Check(LeaveRequestException item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LeaveRequestExceptionRequiredSummaryRule(string Message) : ILeaveRequestExceptionRule
{
    public List<string> Check(LeaveRequestException item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request exception.</summary>
public sealed record LeaveRequestExceptionValidator(List<ILeaveRequestExceptionRule> Rules)
{
    public List<string> Validate(LeaveRequestException item)
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

    public void RequireValid(LeaveRequestException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestException> items)
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

/// <summary>Functions of leave request exception rules.</summary>
public static class LeaveRequestExceptionRules
{
    public static LeaveRequestExceptionValidator DefaultLeaveRequestExceptionValidator()
    {
        return new LeaveRequestExceptionValidator(
            new List<ILeaveRequestExceptionRule>
            {
                new LeaveRequestExceptionLabelLengthRule(1000),
                new LeaveRequestExceptionRequiredLabelRule("leave request exception needs an entry"),
                new LeaveRequestExceptionMaximumRangeRule(0.8, 1741.5),
                new LeaveRequestExceptionRequiredSummaryRule("leave request exception needs this field"),
            });
    }
}
