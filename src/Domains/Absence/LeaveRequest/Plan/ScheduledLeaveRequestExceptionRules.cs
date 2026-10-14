namespace Acme.Workforce.Absence.LeaveRequest.Plan;

/// <summary>A rule a scheduled leave request exception must pass. Check returns its problems, or none.</summary>
public interface IScheduledLeaveRequestExceptionRule
{
    List<string> Check(ScheduledLeaveRequestException item);
}

public sealed record ScheduledLeaveRequestExceptionRequiredCodeRule(string Message) : IScheduledLeaveRequestExceptionRule
{
    public List<string> Check(ScheduledLeaveRequestException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestExceptionCurrencyLengthRule(int MaxLength) : IScheduledLeaveRequestExceptionRule
{
    public List<string> Check(ScheduledLeaveRequestException item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestExceptionTeamLengthRule(int MaxLength) : IScheduledLeaveRequestExceptionRule
{
    public List<string> Check(ScheduledLeaveRequestException item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestExceptionRequiredTeamRule(string Message) : IScheduledLeaveRequestExceptionRule
{
    public List<string> Check(ScheduledLeaveRequestException item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestExceptionRequiredCurrencyRule(string Message) : IScheduledLeaveRequestExceptionRule
{
    public List<string> Check(ScheduledLeaveRequestException item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledLeaveRequestExceptionLabelLengthRule(int MaxLength) : IScheduledLeaveRequestExceptionRule
{
    public List<string> Check(ScheduledLeaveRequestException item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled leave request exception.</summary>
public sealed record ScheduledLeaveRequestExceptionValidator(List<IScheduledLeaveRequestExceptionRule> Rules)
{
    public List<string> Validate(ScheduledLeaveRequestException item)
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

    public void RequireValid(ScheduledLeaveRequestException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledLeaveRequestException> items)
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

/// <summary>Functions of scheduled leave request exception rules.</summary>
public static class ScheduledLeaveRequestExceptionRules
{
    public static ScheduledLeaveRequestExceptionValidator DefaultScheduledLeaveRequestExceptionValidator()
    {
        return new ScheduledLeaveRequestExceptionValidator(
            new List<IScheduledLeaveRequestExceptionRule>
            {
                new ScheduledLeaveRequestExceptionRequiredCodeRule("scheduled leave request exception needs a value"),
                new ScheduledLeaveRequestExceptionCurrencyLengthRule(1000),
                new ScheduledLeaveRequestExceptionTeamLengthRule(16),
                new ScheduledLeaveRequestExceptionRequiredTeamRule(
                    "scheduled leave request exception needs this field"),
                new ScheduledLeaveRequestExceptionRequiredCurrencyRule(
                    "scheduled leave request exception needs an entry"),
                new ScheduledLeaveRequestExceptionLabelLengthRule(86400),
            });
    }
}
