namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet forecast must pass. Check returns its problems, or none.</summary>
public interface ITimesheetForecastRule
{
    List<string> Check(TimesheetForecast item);
}

public sealed record TimesheetForecastCodePrefixRule(string Prefix) : ITimesheetForecastRule
{
    public List<string> Check(TimesheetForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetForecastNameLengthRule(int MaxLength) : ITimesheetForecastRule
{
    public List<string> Check(TimesheetForecast item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetForecastRequiredNameRule(string Message) : ITimesheetForecastRule
{
    public List<string> Check(TimesheetForecast item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetForecastAllowedEmployeeIdRule(List<string> Allowed) : ITimesheetForecastRule
{
    public List<string> Check(TimesheetForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet forecast.</summary>
public sealed record TimesheetForecastValidator(List<ITimesheetForecastRule> Rules)
{
    public List<string> Validate(TimesheetForecast item)
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

    public void RequireValid(TimesheetForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetForecast> items)
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

/// <summary>Functions of timesheet forecast rules.</summary>
public static class TimesheetForecastRules
{
    public static TimesheetForecastValidator DefaultTimesheetForecastValidator()
    {
        return new TimesheetForecastValidator(
            new List<ITimesheetForecastRule>
            {
                new TimesheetForecastCodePrefixRule("X"),
                new TimesheetForecastNameLengthRule(86400),
                new TimesheetForecastRequiredNameRule("timesheet forecast needs a value"),
                new TimesheetForecastAllowedEmployeeIdRule(new List<string> { "tiered", "pending" }),
            });
    }
}
