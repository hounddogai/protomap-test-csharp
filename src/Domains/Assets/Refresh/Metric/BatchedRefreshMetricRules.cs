namespace Acme.Workforce.Assets.Refresh.Metric;

/// <summary>A rule a batched refresh metric must pass. Check returns its problems, or none.</summary>
public interface IBatchedRefreshMetricRule
{
    List<string> Check(BatchedRefreshMetric item);
}

public sealed record BatchedRefreshMetricCapacityRangeRule(int Min, int Max) : IBatchedRefreshMetricRule
{
    public List<string> Check(BatchedRefreshMetric item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRefreshMetricLimitRangeRule(int Min, int Max) : IBatchedRefreshMetricRule
{
    public List<string> Check(BatchedRefreshMetric item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRefreshMetricAmountRangeRule(double Min, double Max) : IBatchedRefreshMetricRule
{
    public List<string> Check(BatchedRefreshMetric item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRefreshMetricCategoryLengthRule(int MaxLength) : IBatchedRefreshMetricRule
{
    public List<string> Check(BatchedRefreshMetric item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRefreshMetricCodePrefixRule(string Prefix) : IBatchedRefreshMetricRule
{
    public List<string> Check(BatchedRefreshMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedRefreshMetricRequiredNotesRule(string Message) : IBatchedRefreshMetricRule
{
    public List<string> Check(BatchedRefreshMetric item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched refresh metric.</summary>
public sealed record BatchedRefreshMetricValidator(List<IBatchedRefreshMetricRule> Rules)
{
    public List<string> Validate(BatchedRefreshMetric item)
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

    public void RequireValid(BatchedRefreshMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedRefreshMetric> items)
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

/// <summary>Functions of batched refresh metric rules.</summary>
public static class BatchedRefreshMetricRules
{
    public static BatchedRefreshMetricValidator DefaultBatchedRefreshMetricValidator()
    {
        return new BatchedRefreshMetricValidator(
            new List<IBatchedRefreshMetricRule>
            {
                new BatchedRefreshMetricCapacityRangeRule(64, 3600),
                new BatchedRefreshMetricLimitRangeRule(32, 120),
                new BatchedRefreshMetricAmountRangeRule(1.4, 3810.3),
                new BatchedRefreshMetricCategoryLengthRule(255),
                new BatchedRefreshMetricCodePrefixRule("REF"),
                new BatchedRefreshMetricRequiredNotesRule("batched refresh metric needs an entry"),
            });
    }
}
