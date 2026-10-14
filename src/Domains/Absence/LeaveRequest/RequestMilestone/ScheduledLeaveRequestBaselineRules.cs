namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

/// <summary>A rule a scheduled leave request baseline must pass. Check returns its problems, or none.</summary>
public interface IScheduledLeaveRequestBaselineRule
{
    List<string> Check(ScheduledLeaveRequestBaseline item);
}

public sealed record ScheduledLeaveRequestBaselineFteRangeRule(double Min, double Max) : IScheduledLeaveRequestBaselineRule
{
    public List<string> Check(ScheduledLeaveRequestBaseline item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledLeaveRequestBaselineCodeLengthRule(int MaxLength) : IScheduledLeaveRequestBaselineRule
{
    public List<string> Check(ScheduledLeaveRequestBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestBaselineRequiredTitleRule(string Message) : IScheduledLeaveRequestBaselineRule
{
    public List<string> Check(ScheduledLeaveRequestBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestBaselineCeilingRangeRule(double Min, double Max) : IScheduledLeaveRequestBaselineRule
{
    public List<string> Check(ScheduledLeaveRequestBaseline item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestBaselineRequiredGradeRule(string Message) : IScheduledLeaveRequestBaselineRule
{
    public List<string> Check(ScheduledLeaveRequestBaseline item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled leave request baseline.</summary>
public sealed record ScheduledLeaveRequestBaselineValidator(List<IScheduledLeaveRequestBaselineRule> Rules)
{
    public List<string> Validate(ScheduledLeaveRequestBaseline item)
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

    public void RequireValid(ScheduledLeaveRequestBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledLeaveRequestBaseline> items)
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

/// <summary>Functions of scheduled leave request baseline rules.</summary>
public static class ScheduledLeaveRequestBaselineRules
{
    public static ScheduledLeaveRequestBaselineValidator DefaultScheduledLeaveRequestBaselineValidator()
    {
        return new ScheduledLeaveRequestBaselineValidator(
            new List<IScheduledLeaveRequestBaselineRule>
            {
                new ScheduledLeaveRequestBaselineFteRangeRule(8.0, 3830.7),
                new ScheduledLeaveRequestBaselineCodeLengthRule(16),
                new ScheduledLeaveRequestBaselineRequiredTitleRule(
                    "scheduled leave request baseline needs this field"),
                new ScheduledLeaveRequestBaselineCeilingRangeRule(2.2, 3134.0),
                new ScheduledLeaveRequestBaselineRequiredGradeRule("scheduled leave request baseline needs a value"),
            });
    }
}
