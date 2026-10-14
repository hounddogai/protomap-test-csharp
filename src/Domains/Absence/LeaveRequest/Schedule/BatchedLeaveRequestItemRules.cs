namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

/// <summary>A rule a batched leave request item must pass. Check returns its problems, or none.</summary>
public interface IBatchedLeaveRequestItemRule
{
    List<string> Check(BatchedLeaveRequestItem item);
}

public sealed record BatchedLeaveRequestItemCeilingRangeRule(double Min, double Max) : IBatchedLeaveRequestItemRule
{
    public List<string> Check(BatchedLeaveRequestItem item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedLeaveRequestItemTitleLengthRule(int MaxLength) : IBatchedLeaveRequestItemRule
{
    public List<string> Check(BatchedLeaveRequestItem item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLeaveRequestItemLabelLengthRule(int MaxLength) : IBatchedLeaveRequestItemRule
{
    public List<string> Check(BatchedLeaveRequestItem item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedLeaveRequestItemTimezoneLengthRule(int MaxLength) : IBatchedLeaveRequestItemRule
{
    public List<string> Check(BatchedLeaveRequestItem item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched leave request item.</summary>
public sealed record BatchedLeaveRequestItemValidator(List<IBatchedLeaveRequestItemRule> Rules)
{
    public List<string> Validate(BatchedLeaveRequestItem item)
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

    public void RequireValid(BatchedLeaveRequestItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedLeaveRequestItem> items)
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

/// <summary>Functions of batched leave request item rules.</summary>
public static class BatchedLeaveRequestItemRules
{
    public static BatchedLeaveRequestItemValidator DefaultBatchedLeaveRequestItemValidator()
    {
        return new BatchedLeaveRequestItemValidator(
            new List<IBatchedLeaveRequestItemRule>
            {
                new BatchedLeaveRequestItemCeilingRangeRule(5.8, 1479.7),
                new BatchedLeaveRequestItemTitleLengthRule(64),
                new BatchedLeaveRequestItemLabelLengthRule(32),
                new BatchedLeaveRequestItemTimezoneLengthRule(32),
            });
    }
}
