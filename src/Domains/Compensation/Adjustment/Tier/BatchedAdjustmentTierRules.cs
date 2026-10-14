namespace Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>A rule a batched adjustment tier must pass. Check returns its problems, or none.</summary>
public interface IBatchedAdjustmentTierRule
{
    List<string> Check(BatchedAdjustmentTier item);
}

public sealed record BatchedAdjustmentTierTenureMonthsRangeRule(int Min, int Max) : IBatchedAdjustmentTierRule
{
    public List<string> Check(BatchedAdjustmentTier item)
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

public sealed record BatchedAdjustmentTierFloorValueRangeRule(double Min, double Max) : IBatchedAdjustmentTierRule
{
    public List<string> Check(BatchedAdjustmentTier item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedAdjustmentTierDiscountRangeRule(double Min, double Max) : IBatchedAdjustmentTierRule
{
    public List<string> Check(BatchedAdjustmentTier item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched adjustment tier.</summary>
public sealed record BatchedAdjustmentTierValidator(List<IBatchedAdjustmentTierRule> Rules)
{
    public List<string> Validate(BatchedAdjustmentTier item)
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

    public void RequireValid(BatchedAdjustmentTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedAdjustmentTier> items)
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

/// <summary>Functions of batched adjustment tier rules.</summary>
public static class BatchedAdjustmentTierRules
{
    public static BatchedAdjustmentTierValidator DefaultBatchedAdjustmentTierValidator()
    {
        return new BatchedAdjustmentTierValidator(
            new List<IBatchedAdjustmentTierRule>
            {
                new BatchedAdjustmentTierTenureMonthsRangeRule(16, 255),
                new BatchedAdjustmentTierFloorValueRangeRule(1.2, 1888.6),
                new BatchedAdjustmentTierDiscountRangeRule(0.8, 559.6),
            });
    }
}
