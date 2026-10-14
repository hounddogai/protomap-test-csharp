namespace Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>A rule a scheduled leave request override must pass. Check returns its problems, or none.</summary>
public interface IScheduledLeaveRequestOverrideRule
{
    List<string> Check(ScheduledLeaveRequestOverride item);
}

public sealed record ScheduledLeaveRequestOverrideRequiredJobCodeRule(string Message) : IScheduledLeaveRequestOverrideRule
{
    public List<string> Check(ScheduledLeaveRequestOverride item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestOverrideRequiredLocaleRule(string Message) : IScheduledLeaveRequestOverrideRule
{
    public List<string> Check(ScheduledLeaveRequestOverride item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestOverrideRatingRangeRule(double Min, double Max) : IScheduledLeaveRequestOverrideRule
{
    public List<string> Check(ScheduledLeaveRequestOverride item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled leave request override.</summary>
public sealed record ScheduledLeaveRequestOverrideValidator(List<IScheduledLeaveRequestOverrideRule> Rules)
{
    public List<string> Validate(ScheduledLeaveRequestOverride item)
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

    public void RequireValid(ScheduledLeaveRequestOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledLeaveRequestOverride> items)
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

/// <summary>Functions of scheduled leave request override rules.</summary>
public static class ScheduledLeaveRequestOverrideRules
{
    public static ScheduledLeaveRequestOverrideValidator DefaultScheduledLeaveRequestOverrideValidator()
    {
        return new ScheduledLeaveRequestOverrideValidator(
            new List<IScheduledLeaveRequestOverrideRule>
            {
                new ScheduledLeaveRequestOverrideRequiredJobCodeRule(
                    "scheduled leave request override needs a value"),
                new ScheduledLeaveRequestOverrideRequiredLocaleRule(
                    "scheduled leave request override needs this field"),
                new ScheduledLeaveRequestOverrideRatingRangeRule(5.2, 3955.7),
            });
    }
}
