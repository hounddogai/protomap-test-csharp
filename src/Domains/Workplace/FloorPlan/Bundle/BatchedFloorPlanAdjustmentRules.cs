namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

/// <summary>A rule a batched floor plan adjustment must pass. Check returns its problems, or none.</summary>
public interface IBatchedFloorPlanAdjustmentRule
{
    List<string> Check(BatchedFloorPlanAdjustment item);
}

public sealed record BatchedFloorPlanAdjustmentStartsAtRangeRule(int Min, int Max) : IBatchedFloorPlanAdjustmentRule
{
    public List<string> Check(BatchedFloorPlanAdjustment item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedFloorPlanAdjustmentNameLengthRule(int MaxLength) : IBatchedFloorPlanAdjustmentRule
{
    public List<string> Check(BatchedFloorPlanAdjustment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedFloorPlanAdjustmentRatioRangeRule(double Min, double Max) : IBatchedFloorPlanAdjustmentRule
{
    public List<string> Check(BatchedFloorPlanAdjustment item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedFloorPlanAdjustmentRateRangeRule(double Min, double Max) : IBatchedFloorPlanAdjustmentRule
{
    public List<string> Check(BatchedFloorPlanAdjustment item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedFloorPlanAdjustmentCodePrefixRule(string Prefix) : IBatchedFloorPlanAdjustmentRule
{
    public List<string> Check(BatchedFloorPlanAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedFloorPlanAdjustmentRequiredReviewerRule(string Message) : IBatchedFloorPlanAdjustmentRule
{
    public List<string> Check(BatchedFloorPlanAdjustment item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched floor plan adjustment.</summary>
public sealed record BatchedFloorPlanAdjustmentValidator(List<IBatchedFloorPlanAdjustmentRule> Rules)
{
    public List<string> Validate(BatchedFloorPlanAdjustment item)
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

    public void RequireValid(BatchedFloorPlanAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedFloorPlanAdjustment> items)
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

/// <summary>Functions of batched floor plan adjustment rules.</summary>
public static class BatchedFloorPlanAdjustmentRules
{
    public static BatchedFloorPlanAdjustmentValidator DefaultBatchedFloorPlanAdjustmentValidator()
    {
        return new BatchedFloorPlanAdjustmentValidator(
            new List<IBatchedFloorPlanAdjustmentRule>
            {
                new BatchedFloorPlanAdjustmentStartsAtRangeRule(120, 3600),
                new BatchedFloorPlanAdjustmentNameLengthRule(64),
                new BatchedFloorPlanAdjustmentRatioRangeRule(9.4, 2422.7),
                new BatchedFloorPlanAdjustmentRateRangeRule(8.7, 1219.1),
                new BatchedFloorPlanAdjustmentCodePrefixRule("OPS"),
                new BatchedFloorPlanAdjustmentRequiredReviewerRule("batched floor plan adjustment needs a value"),
            });
    }
}
