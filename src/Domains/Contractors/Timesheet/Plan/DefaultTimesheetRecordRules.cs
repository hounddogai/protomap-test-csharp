namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default timesheet record must pass. Check returns its problems, or none.</summary>
public interface IDefaultTimesheetRecordRule
{
    List<string> Check(DefaultTimesheetRecord item);
}

public sealed record DefaultTimesheetRecordRequiredNameRule(string Message) : IDefaultTimesheetRecordRule
{
    public List<string> Check(DefaultTimesheetRecord item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultTimesheetRecordAllowedSourceRule(List<string> Allowed) : IDefaultTimesheetRecordRule
{
    public List<string> Check(DefaultTimesheetRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record DefaultTimesheetRecordVisibleGuardRule(double Limit) : IDefaultTimesheetRecordRule
{
    public List<string> Check(DefaultTimesheetRecord item)
    {
        if (item.Visible && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record DefaultTimesheetRecordGraceDaysRangeRule(int Min, int Max) : IDefaultTimesheetRecordRule
{
    public List<string> Check(DefaultTimesheetRecord item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default timesheet record.</summary>
public sealed record DefaultTimesheetRecordValidator(List<IDefaultTimesheetRecordRule> Rules)
{
    public List<string> Validate(DefaultTimesheetRecord item)
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

    public void RequireValid(DefaultTimesheetRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultTimesheetRecord> items)
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

/// <summary>Functions of default timesheet record rules.</summary>
public static class DefaultTimesheetRecordRules
{
    public static DefaultTimesheetRecordValidator DefaultDefaultTimesheetRecordValidator()
    {
        return new DefaultTimesheetRecordValidator(
            new List<IDefaultTimesheetRecordRule>
            {
                new DefaultTimesheetRecordRequiredNameRule("default timesheet record needs this field"),
                new DefaultTimesheetRecordAllowedSourceRule(new List<string> { "default", "manual" }),
                new DefaultTimesheetRecordVisibleGuardRule(6.5),
                new DefaultTimesheetRecordGraceDaysRangeRule(86400, 86400),
            });
    }
}
