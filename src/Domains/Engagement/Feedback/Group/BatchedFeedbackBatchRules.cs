namespace Acme.Workforce.Engagement.Feedback.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched feedback batch must pass. Check returns its problems, or none.</summary>
public interface IBatchedFeedbackBatchRule
{
    List<string> Check(BatchedFeedbackBatch item);
}

public sealed record BatchedFeedbackBatchRequiredCodeRule(string Message) : IBatchedFeedbackBatchRule
{
    public List<string> Check(BatchedFeedbackBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackBatchRequiredSegmentRule(string Message) : IBatchedFeedbackBatchRule
{
    public List<string> Check(BatchedFeedbackBatch item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackBatchCategoryLengthRule(int MaxLength) : IBatchedFeedbackBatchRule
{
    public List<string> Check(BatchedFeedbackBatch item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackBatchAmountRangeRule(double Min, double Max) : IBatchedFeedbackBatchRule
{
    public List<string> Check(BatchedFeedbackBatch item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedFeedbackBatchAllowedCategoryRule(List<string> Allowed) : IBatchedFeedbackBatchRule
{
    public List<string> Check(BatchedFeedbackBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record BatchedFeedbackBatchCodeLengthRule(int MaxLength) : IBatchedFeedbackBatchRule
{
    public List<string> Check(BatchedFeedbackBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched feedback batch.</summary>
public sealed record BatchedFeedbackBatchValidator(List<IBatchedFeedbackBatchRule> Rules)
{
    public List<string> Validate(BatchedFeedbackBatch item)
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

    public void RequireValid(BatchedFeedbackBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedFeedbackBatch> items)
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

/// <summary>Functions of batched feedback batch rules.</summary>
public static class BatchedFeedbackBatchRules
{
    public static BatchedFeedbackBatchValidator DefaultBatchedFeedbackBatchValidator()
    {
        return new BatchedFeedbackBatchValidator(
            new List<IBatchedFeedbackBatchRule>
            {
                new BatchedFeedbackBatchRequiredCodeRule("batched feedback batch needs this field"),
                new BatchedFeedbackBatchRequiredSegmentRule("batched feedback batch needs a value"),
                new BatchedFeedbackBatchCategoryLengthRule(80),
                new BatchedFeedbackBatchAmountRangeRule(0.9, 775.3),
                new BatchedFeedbackBatchAllowedCategoryRule(new List<string>()),
                new BatchedFeedbackBatchCodeLengthRule(32),
            });
    }
}
