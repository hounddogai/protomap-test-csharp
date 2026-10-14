namespace Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>A rule a batched feedback baseline must pass. Check returns its problems, or none.</summary>
public interface IBatchedFeedbackBaselineRule
{
    List<string> Check(BatchedFeedbackBaseline item);
}

public sealed record BatchedFeedbackBaselineEndsAtRangeRule(int Min, int Max) : IBatchedFeedbackBaselineRule
{
    public List<string> Check(BatchedFeedbackBaseline item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackBaselineSegmentLengthRule(int MaxLength) : IBatchedFeedbackBaselineRule
{
    public List<string> Check(BatchedFeedbackBaseline item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackBaselineRequiredTitleRule(string Message) : IBatchedFeedbackBaselineRule
{
    public List<string> Check(BatchedFeedbackBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackBaselineDepartmentLengthRule(int MaxLength) : IBatchedFeedbackBaselineRule
{
    public List<string> Check(BatchedFeedbackBaseline item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched feedback baseline.</summary>
public sealed record BatchedFeedbackBaselineValidator(List<IBatchedFeedbackBaselineRule> Rules)
{
    public List<string> Validate(BatchedFeedbackBaseline item)
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

    public void RequireValid(BatchedFeedbackBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedFeedbackBaseline> items)
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

/// <summary>Functions of batched feedback baseline rules.</summary>
public static class BatchedFeedbackBaselineRules
{
    public static BatchedFeedbackBaselineValidator DefaultBatchedFeedbackBaselineValidator()
    {
        return new BatchedFeedbackBaselineValidator(
            new List<IBatchedFeedbackBaselineRule>
            {
                new BatchedFeedbackBaselineEndsAtRangeRule(32, 86400),
                new BatchedFeedbackBaselineSegmentLengthRule(3600),
                new BatchedFeedbackBaselineRequiredTitleRule("batched feedback baseline needs an entry"),
                new BatchedFeedbackBaselineDepartmentLengthRule(32),
            });
    }
}
