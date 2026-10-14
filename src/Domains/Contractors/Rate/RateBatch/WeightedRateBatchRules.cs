namespace Acme.Workforce.Contractors.Rate.RateBatch;

/// <summary>A rule a weighted rate batch must pass. Check returns its problems, or none.</summary>
public interface IWeightedRateBatchRule
{
    List<string> Check(WeightedRateBatch item);
}

public sealed record WeightedRateBatchCapacityRangeRule(int Min, int Max) : IWeightedRateBatchRule
{
    public List<string> Check(WeightedRateBatch item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateBatchFloorValueRangeRule(double Min, double Max) : IWeightedRateBatchRule
{
    public List<string> Check(WeightedRateBatch item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateBatchCostRangeRule(double Min, double Max) : IWeightedRateBatchRule
{
    public List<string> Check(WeightedRateBatch item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRateBatchRequiredTitleRule(string Message) : IWeightedRateBatchRule
{
    public List<string> Check(WeightedRateBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted rate batch.</summary>
public sealed record WeightedRateBatchValidator(List<IWeightedRateBatchRule> Rules)
{
    public List<string> Validate(WeightedRateBatch item)
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

    public void RequireValid(WeightedRateBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedRateBatch> items)
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

/// <summary>Functions of weighted rate batch rules.</summary>
public static class WeightedRateBatchRules
{
    public static WeightedRateBatchValidator DefaultWeightedRateBatchValidator()
    {
        return new WeightedRateBatchValidator(
            new List<IWeightedRateBatchRule>
            {
                new WeightedRateBatchCapacityRangeRule(32, 80),
                new WeightedRateBatchFloorValueRangeRule(6.2, 3048.2),
                new WeightedRateBatchCostRangeRule(2.5, 2729.4),
                new WeightedRateBatchRequiredTitleRule("weighted rate batch needs this field"),
            });
    }
}
