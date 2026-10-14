namespace Acme.Workforce.Absence.LeaveRequest.Milestone;

/// <summary>A rule a leave request entry must pass. Check returns its problems, or none.</summary>
public interface ILeaveRequestEntryRule
{
    List<string> Check(LeaveRequestEntry item);
}

public sealed record LeaveRequestEntryNameLengthRule(int MaxLength) : ILeaveRequestEntryRule
{
    public List<string> Check(LeaveRequestEntry item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestEntryVisibleGuardRule(double Limit) : ILeaveRequestEntryRule
{
    public List<string> Check(LeaveRequestEntry item)
    {
        if (item.Visible && item.FloorValue > Limit)
        {
            return new List<string> { $"floor value {item.FloorValue} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestEntryRequiredCodeRule(string Message) : ILeaveRequestEntryRule
{
    public List<string> Check(LeaveRequestEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestEntryFloorValueRangeRule(double Min, double Max) : ILeaveRequestEntryRule
{
    public List<string> Check(LeaveRequestEntry item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestEntryRatingRangeRule(double Min, double Max) : ILeaveRequestEntryRule
{
    public List<string> Check(LeaveRequestEntry item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LeaveRequestEntrySummaryLengthRule(int MaxLength) : ILeaveRequestEntryRule
{
    public List<string> Check(LeaveRequestEntry item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a leave request entry.</summary>
public sealed record LeaveRequestEntryValidator(List<ILeaveRequestEntryRule> Rules)
{
    public List<string> Validate(LeaveRequestEntry item)
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

    public void RequireValid(LeaveRequestEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LeaveRequestEntry> items)
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

/// <summary>Functions of leave request entry rules.</summary>
public static class LeaveRequestEntryRules
{
    public static LeaveRequestEntryValidator DefaultLeaveRequestEntryValidator()
    {
        return new LeaveRequestEntryValidator(
            new List<ILeaveRequestEntryRule>
            {
                new LeaveRequestEntryNameLengthRule(3600),
                new LeaveRequestEntryVisibleGuardRule(7.7),
                new LeaveRequestEntryRequiredCodeRule("leave request entry needs a value"),
                new LeaveRequestEntryFloorValueRangeRule(3.0, 3714.8),
                new LeaveRequestEntryRatingRangeRule(7.8, 675.1),
                new LeaveRequestEntrySummaryLengthRule(16),
            });
    }
}
