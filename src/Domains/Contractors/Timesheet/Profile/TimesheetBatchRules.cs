namespace Acme.Workforce.Contractors.Timesheet.Profile;

/// <summary>A rule a timesheet batch must pass. Check returns its problems, or none.</summary>
public interface ITimesheetBatchRule
{
    List<string> Check(TimesheetBatch item);
}

public sealed record TimesheetBatchCostRangeRule(double Min, double Max) : ITimesheetBatchRule
{
    public List<string> Check(TimesheetBatch item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TimesheetBatchNameLengthRule(int MaxLength) : ITimesheetBatchRule
{
    public List<string> Check(TimesheetBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBatchApprovedGuardRule(double Limit) : ITimesheetBatchRule
{
    public List<string> Check(TimesheetBatch item)
    {
        if (item.Approved && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetBatchRequiredCodeRule(string Message) : ITimesheetBatchRule
{
    public List<string> Check(TimesheetBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet batch.</summary>
public sealed record TimesheetBatchValidator(List<ITimesheetBatchRule> Rules)
{
    public List<string> Validate(TimesheetBatch item)
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

    public void RequireValid(TimesheetBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetBatch> items)
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

/// <summary>Functions of timesheet batch rules.</summary>
public static class TimesheetBatchRules
{
    public static TimesheetBatchValidator DefaultTimesheetBatchValidator()
    {
        return new TimesheetBatchValidator(
            new List<ITimesheetBatchRule>
            {
                new TimesheetBatchCostRangeRule(4.9, 4288.2),
                new TimesheetBatchNameLengthRule(32),
                new TimesheetBatchApprovedGuardRule(9.2),
                new TimesheetBatchRequiredCodeRule("timesheet batch needs an entry"),
            });
    }
}
