namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

/// <summary>A rule a leave request record must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestRecordRule
{
    List<string> Check(LeaveRequestRecord item);
}

public sealed record LeaveRequestRecordRequiredLabelRule(string Message) : ILeaveRequestRecordRule
{
    public List<string> Check(LeaveRequestRecord item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestRecordManagerIdLengthRule(int MaxLength) : ILeaveRequestRecordRule
{
    public List<string> Check(LeaveRequestRecord item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestRecordRequiredManagerIdRule(string Message) : ILeaveRequestRecordRule
{
    public List<string> Check(LeaveRequestRecord item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestRecordRequiredCodeRule(string Message) : ILeaveRequestRecordRule
{
    public List<string> Check(LeaveRequestRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestRecordGradeLengthRule(int MaxLength) : ILeaveRequestRecordRule
{
    public List<string> Check(LeaveRequestRecord item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestRecordCodePrefixRule(string Prefix) : ILeaveRequestRecordRule
{
    public List<string> Check(LeaveRequestRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request record.</summary>
public sealed record LeaveRequestRecordValidator(List<ILeaveRequestRecordRule> Rules)
{
    public List<string> Validate(LeaveRequestRecord item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(LeaveRequestRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestRecord> items)
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

/// <summary>Functions of leave request record rules.</summary>
public static class LeaveRequestRecordRules
{
    public static LeaveRequestRecordValidator DefaultLeaveRequestRecordValidator()
    {
        return new LeaveRequestRecordValidator(
            new List<ILeaveRequestRecordRule>
            {
                new LeaveRequestRecordRequiredLabelRule("leave request record needs a value"),
                new LeaveRequestRecordManagerIdLengthRule(1000),
                new LeaveRequestRecordRequiredManagerIdRule("leave request record needs an entry"),
                new LeaveRequestRecordRequiredCodeRule("leave request record needs this field"),
                new LeaveRequestRecordGradeLengthRule(3600),
                new LeaveRequestRecordCodePrefixRule("OPS"),
            });
    }
}
