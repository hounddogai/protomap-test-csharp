namespace Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>A rule a batched engagement exception must pass. Check returns its problems, or none.</summary>
public interface IBatchedEngagementExceptionRule
{
    List<string> Check(BatchedEngagementException item);
}

public sealed record BatchedEngagementExceptionNameLengthRule(int MaxLength) : IBatchedEngagementExceptionRule
{
    public List<string> Check(BatchedEngagementException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedEngagementExceptionWindowDaysRangeRule(int Min, int Max) : IBatchedEngagementExceptionRule
{
    public List<string> Check(BatchedEngagementException item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedEngagementExceptionTenureMonthsRangeRule(int Min, int Max) : IBatchedEngagementExceptionRule
{
    public List<string> Check(BatchedEngagementException item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a batched engagement exception.</summary>
public sealed record BatchedEngagementExceptionValidator(List<IBatchedEngagementExceptionRule> Rules)
{
    public List<string> Validate(BatchedEngagementException item)
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

    public void RequireValid(BatchedEngagementException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedEngagementException> items)
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

/// <summary>Functions of batched engagement exception rules.</summary>
public static class BatchedEngagementExceptionRules
{
    public static BatchedEngagementExceptionValidator DefaultBatchedEngagementExceptionValidator()
    {
        return new BatchedEngagementExceptionValidator(
            new List<IBatchedEngagementExceptionRule>
            {
                new BatchedEngagementExceptionNameLengthRule(16),
                new BatchedEngagementExceptionWindowDaysRangeRule(120, 255),
                new BatchedEngagementExceptionTenureMonthsRangeRule(32, 255),
            });
    }
}
