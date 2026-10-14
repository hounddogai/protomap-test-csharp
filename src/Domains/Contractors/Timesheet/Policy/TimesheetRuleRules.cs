namespace Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>A rule a timesheet rule must pass. Check returns its problems, or none.</summary>
public interface ITimesheetRuleRule
{
    List<string> Check(TimesheetRule item);
}

public sealed record TimesheetRuleCodePrefixRule(string Prefix) : ITimesheetRuleRule
{
    public List<string> Check(TimesheetRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetRuleMarginRangeRule(double Min, double Max) : ITimesheetRuleRule
{
    public List<string> Check(TimesheetRule item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetRuleRequiredTimezoneRule(string Message) : ITimesheetRuleRule
{
    public List<string> Check(TimesheetRule item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetRuleRequiredTitleRule(string Message) : ITimesheetRuleRule
{
    public List<string> Check(TimesheetRule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet rule.</summary>
public sealed record TimesheetRuleValidator(List<ITimesheetRuleRule> Rules)
{
    public List<string> Validate(TimesheetRule item)
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

    public void RequireValid(TimesheetRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetRule> items)
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

/// <summary>Functions of timesheet rule rules.</summary>
public static class TimesheetRuleRules
{
    public static TimesheetRuleValidator DefaultTimesheetRuleValidator()
    {
        return new TimesheetRuleValidator(
            new List<ITimesheetRuleRule>
            {
                new TimesheetRuleCodePrefixRule("REF"),
                new TimesheetRuleMarginRangeRule(7.5, 1408.1),
                new TimesheetRuleRequiredTimezoneRule("timesheet rule needs a value"),
                new TimesheetRuleRequiredTitleRule("timesheet rule needs a value"),
            });
    }
}
