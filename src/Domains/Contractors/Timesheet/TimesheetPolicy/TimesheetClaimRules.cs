namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

/// <summary>A rule a timesheet claim must pass. Check returns its problems, or none.</summary>
public interface ITimesheetClaimRule
{
    List<string> Check(TimesheetClaim item);
}

public sealed record TimesheetClaimCurrencyLengthRule(int MaxLength) : ITimesheetClaimRule
{
    public List<string> Check(TimesheetClaim item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetClaimRequiredCodeRule(string Message) : ITimesheetClaimRule
{
    public List<string> Check(TimesheetClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetClaimUpdatedAtRangeRule(int Min, int Max) : ITimesheetClaimRule
{
    public List<string> Check(TimesheetClaim item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TimesheetClaimTitleLengthRule(int MaxLength) : ITimesheetClaimRule
{
    public List<string> Check(TimesheetClaim item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet claim.</summary>
public sealed record TimesheetClaimValidator(List<ITimesheetClaimRule> Rules)
{
    public List<string> Validate(TimesheetClaim item)
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

    public void RequireValid(TimesheetClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetClaim> items)
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

/// <summary>Functions of timesheet claim rules.</summary>
public static class TimesheetClaimRules
{
    public static TimesheetClaimValidator DefaultTimesheetClaimValidator()
    {
        return new TimesheetClaimValidator(
            new List<ITimesheetClaimRule>
            {
                new TimesheetClaimCurrencyLengthRule(64),
                new TimesheetClaimRequiredCodeRule("timesheet claim needs a value"),
                new TimesheetClaimUpdatedAtRangeRule(16, 32),
                new TimesheetClaimTitleLengthRule(32),
            });
    }
}
