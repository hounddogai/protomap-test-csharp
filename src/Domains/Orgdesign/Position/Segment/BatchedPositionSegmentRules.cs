namespace Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>A rule a batched position segment must pass. Check returns its problems, or none.</summary>
public interface IBatchedPositionSegmentRule
{
    List<string> Check(BatchedPositionSegment item);
}

public sealed record BatchedPositionSegmentAttemptsRangeRule(int Min, int Max) : IBatchedPositionSegmentRule
{
    public List<string> Check(BatchedPositionSegment item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedPositionSegmentCapacityRangeRule(int Min, int Max) : IBatchedPositionSegmentRule
{
    public List<string> Check(BatchedPositionSegment item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedPositionSegmentRequiredReviewerRule(string Message) : IBatchedPositionSegmentRule
{
    public List<string> Check(BatchedPositionSegment item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedPositionSegmentReviewerLengthRule(int MaxLength) : IBatchedPositionSegmentRule
{
    public List<string> Check(BatchedPositionSegment item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPositionSegmentBudgetRangeRule(double Min, double Max) : IBatchedPositionSegmentRule
{
    public List<string> Check(BatchedPositionSegment item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched position segment.</summary>
public sealed record BatchedPositionSegmentValidator(List<IBatchedPositionSegmentRule> Rules)
{
    public List<string> Validate(BatchedPositionSegment item)
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

    public void RequireValid(BatchedPositionSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedPositionSegment> items)
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

/// <summary>Functions of batched position segment rules.</summary>
public static class BatchedPositionSegmentRules
{
    public static BatchedPositionSegmentValidator DefaultBatchedPositionSegmentValidator()
    {
        return new BatchedPositionSegmentValidator(
            new List<IBatchedPositionSegmentRule>
            {
                new BatchedPositionSegmentAttemptsRangeRule(64, 3600),
                new BatchedPositionSegmentCapacityRangeRule(64, 3600),
                new BatchedPositionSegmentRequiredReviewerRule("batched position segment needs a value"),
                new BatchedPositionSegmentReviewerLengthRule(1000),
                new BatchedPositionSegmentBudgetRangeRule(0.9, 102.6),
            });
    }
}
