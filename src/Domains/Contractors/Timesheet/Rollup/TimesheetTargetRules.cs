namespace Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>A rule a timesheet target must pass. Check returns its problems, or none.</summary>
public interface ITimesheetTargetRule
{
    List<string> Check(TimesheetTarget item);
}

public sealed record TimesheetTargetRequiredPhoneRule(string Message) : ITimesheetTargetRule
{
    public List<string> Check(TimesheetTarget item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetTargetCodePrefixRule(string Prefix) : ITimesheetTargetRule
{
    public List<string> Check(TimesheetTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetTargetPhoneLengthRule(int MaxLength) : ITimesheetTargetRule
{
    public List<string> Check(TimesheetTarget item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet target.</summary>
public sealed record TimesheetTargetValidator(List<ITimesheetTargetRule> Rules)
{
    public List<string> Validate(TimesheetTarget item)
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

    public void RequireValid(TimesheetTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetTarget> items)
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

/// <summary>Functions of timesheet target rules.</summary>
public static class TimesheetTargetRules
{
    public static TimesheetTargetValidator DefaultTimesheetTargetValidator()
    {
        return new TimesheetTargetValidator(
            new List<ITimesheetTargetRule>
            {
                new TimesheetTargetRequiredPhoneRule("timesheet target needs this field"),
                new TimesheetTargetCodePrefixRule("Q"),
                new TimesheetTargetPhoneLengthRule(255),
            });
    }
}
