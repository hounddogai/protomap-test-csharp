namespace Acme.Workforce.Contractors.Timesheet.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet exception must pass. Check returns its problems, or none.</summary>
public interface ITimesheetExceptionRule
{
    List<string> Check(TimesheetException item);
}

public sealed record TimesheetExceptionMinimumRangeRule(double Min, double Max) : ITimesheetExceptionRule
{
    public List<string> Check(TimesheetException item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetExceptionCodePrefixRule(string Prefix) : ITimesheetExceptionRule
{
    public List<string> Check(TimesheetException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetExceptionQuantityRangeRule(int Min, int Max) : ITimesheetExceptionRule
{
    public List<string> Check(TimesheetException item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetExceptionRequiredCurrencyRule(string Message) : ITimesheetExceptionRule
{
    public List<string> Check(TimesheetException item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetExceptionAllowedTitleRule(List<string> Allowed) : ITimesheetExceptionRule
{
    public List<string> Check(TimesheetException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet exception.</summary>
public sealed record TimesheetExceptionValidator(List<ITimesheetExceptionRule> Rules)
{
    public List<string> Validate(TimesheetException item)
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

    public void RequireValid(TimesheetException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetException> items)
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

/// <summary>Functions of timesheet exception rules.</summary>
public static class TimesheetExceptionRules
{
    public static TimesheetExceptionValidator DefaultTimesheetExceptionValidator()
    {
        return new TimesheetExceptionValidator(
            new List<ITimesheetExceptionRule>
            {
                new TimesheetExceptionMinimumRangeRule(4.1, 519.7),
                new TimesheetExceptionCodePrefixRule("Q"),
                new TimesheetExceptionQuantityRangeRule(16, 120),
                new TimesheetExceptionRequiredCurrencyRule("timesheet exception needs this field"),
                new TimesheetExceptionAllowedTitleRule(new List<string> { "primary" }),
            });
    }
}
