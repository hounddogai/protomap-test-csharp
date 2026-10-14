namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

/// <summary>A rule a timesheet template must pass. Check returns its problems, or none.</summary>
public interface ITimesheetTemplateRule
{
    List<string> Check(TimesheetTemplate item);
}

public sealed record TimesheetTemplateCodePrefixRule(string Prefix) : ITimesheetTemplateRule
{
    public List<string> Check(TimesheetTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetTemplateSegmentLengthRule(int MaxLength) : ITimesheetTemplateRule
{
    public List<string> Check(TimesheetTemplate item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetTemplateRequiredSourceRule(string Message) : ITimesheetTemplateRule
{
    public List<string> Check(TimesheetTemplate item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetTemplateNameLengthRule(int MaxLength) : ITimesheetTemplateRule
{
    public List<string> Check(TimesheetTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet template.</summary>
public sealed record TimesheetTemplateValidator(List<ITimesheetTemplateRule> Rules)
{
    public List<string> Validate(TimesheetTemplate item)
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

    public void RequireValid(TimesheetTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetTemplate> items)
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

/// <summary>Functions of timesheet template rules.</summary>
public static class TimesheetTemplateRules
{
    public static TimesheetTemplateValidator DefaultTimesheetTemplateValidator()
    {
        return new TimesheetTemplateValidator(
            new List<ITimesheetTemplateRule>
            {
                new TimesheetTemplateCodePrefixRule("ACM"),
                new TimesheetTemplateSegmentLengthRule(64),
                new TimesheetTemplateRequiredSourceRule("timesheet template needs an entry"),
                new TimesheetTemplateNameLengthRule(120),
            });
    }
}
