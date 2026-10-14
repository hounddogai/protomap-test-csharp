namespace Acme.Workforce.Contractors.Timesheet.Batch;

/// <summary>A rule a global timesheet batch must pass. Check returns its problems, or none.</summary>
public interface IGlobalTimesheetBatchRule
{
    List<string> Check(GlobalTimesheetBatch item);
}

public sealed record GlobalTimesheetBatchTitleLengthRule(int MaxLength) : IGlobalTimesheetBatchRule
{
    public List<string> Check(GlobalTimesheetBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalTimesheetBatchRequiredTitleRule(string Message) : IGlobalTimesheetBatchRule
{
    public List<string> Check(GlobalTimesheetBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalTimesheetBatchNameLengthRule(int MaxLength) : IGlobalTimesheetBatchRule
{
    public List<string> Check(GlobalTimesheetBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalTimesheetBatchRequiredCodeRule(string Message) : IGlobalTimesheetBatchRule
{
    public List<string> Check(GlobalTimesheetBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global timesheet batch.</summary>
public sealed record GlobalTimesheetBatchValidator(List<IGlobalTimesheetBatchRule> Rules)
{
    public List<string> Validate(GlobalTimesheetBatch item)
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

    public void RequireValid(GlobalTimesheetBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalTimesheetBatch> items)
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

/// <summary>Functions of global timesheet batch rules.</summary>
public static class GlobalTimesheetBatchRules
{
    public static GlobalTimesheetBatchValidator DefaultGlobalTimesheetBatchValidator()
    {
        return new GlobalTimesheetBatchValidator(
            new List<IGlobalTimesheetBatchRule>
            {
                new GlobalTimesheetBatchTitleLengthRule(16),
                new GlobalTimesheetBatchRequiredTitleRule("global timesheet batch needs a value"),
                new GlobalTimesheetBatchNameLengthRule(16),
                new GlobalTimesheetBatchRequiredCodeRule("global timesheet batch needs this field"),
            });
    }
}
