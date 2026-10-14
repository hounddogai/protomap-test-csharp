namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>A rule a batched timesheet estimate must pass. Check returns its problems, or none.</summary>
public interface IBatchedTimesheetEstimateRule
{
    List<string> Check(BatchedTimesheetEstimate item);
}

public sealed record BatchedTimesheetEstimateRequiredCodeRule(string Message) : IBatchedTimesheetEstimateRule
{
    public List<string> Check(BatchedTimesheetEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTimesheetEstimateManagerIdLengthRule(int MaxLength) : IBatchedTimesheetEstimateRule
{
    public List<string> Check(BatchedTimesheetEstimate item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTimesheetEstimateRatingRangeRule(double Min, double Max) : IBatchedTimesheetEstimateRule
{
    public List<string> Check(BatchedTimesheetEstimate item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTimesheetEstimateCodeLengthRule(int MaxLength) : IBatchedTimesheetEstimateRule
{
    public List<string> Check(BatchedTimesheetEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTimesheetEstimateNotesLengthRule(int MaxLength) : IBatchedTimesheetEstimateRule
{
    public List<string> Check(BatchedTimesheetEstimate item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched timesheet estimate.</summary>
public sealed record BatchedTimesheetEstimateValidator(List<IBatchedTimesheetEstimateRule> Rules)
{
    public List<string> Validate(BatchedTimesheetEstimate item)
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

    public void RequireValid(BatchedTimesheetEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedTimesheetEstimate> items)
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

/// <summary>Functions of batched timesheet estimate rules.</summary>
public static class BatchedTimesheetEstimateRules
{
    public static BatchedTimesheetEstimateValidator DefaultBatchedTimesheetEstimateValidator()
    {
        return new BatchedTimesheetEstimateValidator(
            new List<IBatchedTimesheetEstimateRule>
            {
                new BatchedTimesheetEstimateRequiredCodeRule("batched timesheet estimate needs a value"),
                new BatchedTimesheetEstimateManagerIdLengthRule(86400),
                new BatchedTimesheetEstimateRatingRangeRule(6.0, 3489.8),
                new BatchedTimesheetEstimateCodeLengthRule(86400),
                new BatchedTimesheetEstimateNotesLengthRule(255),
            });
    }
}
