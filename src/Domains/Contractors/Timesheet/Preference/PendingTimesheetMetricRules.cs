namespace Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>A rule a pending timesheet metric must pass. Check returns its problems, or none.</summary>
public interface IPendingTimesheetMetricRule
{
    List<string> Check(PendingTimesheetMetric item);
}

public sealed record PendingTimesheetMetricLabelLengthRule(int MaxLength) : IPendingTimesheetMetricRule
{
    public List<string> Check(PendingTimesheetMetric item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingTimesheetMetricRequiredCountryRule(string Message) : IPendingTimesheetMetricRule
{
    public List<string> Check(PendingTimesheetMetric item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingTimesheetMetricPriorityRangeRule(int Min, int Max) : IPendingTimesheetMetricRule
{
    public List<string> Check(PendingTimesheetMetric item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pending timesheet metric.</summary>
public sealed record PendingTimesheetMetricValidator(List<IPendingTimesheetMetricRule> Rules)
{
    public List<string> Validate(PendingTimesheetMetric item)
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

    public void RequireValid(PendingTimesheetMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingTimesheetMetric> items)
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

/// <summary>Functions of pending timesheet metric rules.</summary>
public static class PendingTimesheetMetricRules
{
    public static PendingTimesheetMetricValidator DefaultPendingTimesheetMetricValidator()
    {
        return new PendingTimesheetMetricValidator(
            new List<IPendingTimesheetMetricRule>
            {
                new PendingTimesheetMetricLabelLengthRule(32),
                new PendingTimesheetMetricRequiredCountryRule("pending timesheet metric needs this field"),
                new PendingTimesheetMetricPriorityRangeRule(16, 255),
            });
    }
}
