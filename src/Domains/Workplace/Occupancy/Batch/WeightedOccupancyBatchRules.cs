namespace Acme.Workforce.Workplace.Occupancy.Batch;

/// <summary>A rule a weighted occupancy batch must pass. Check returns its problems, or none.</summary>
public interface IWeightedOccupancyBatchRule
{
    List<string> Check(WeightedOccupancyBatch item);
}

public sealed record WeightedOccupancyBatchCategoryLengthRule(int MaxLength) : IWeightedOccupancyBatchRule
{
    public List<string> Check(WeightedOccupancyBatch item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedOccupancyBatchFloorValueRangeRule(double Min, double Max) : IWeightedOccupancyBatchRule
{
    public List<string> Check(WeightedOccupancyBatch item)
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

public sealed record WeightedOccupancyBatchTimezoneLengthRule(int MaxLength) : IWeightedOccupancyBatchRule
{
    public List<string> Check(WeightedOccupancyBatch item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedOccupancyBatchCeilingRangeRule(double Min, double Max) : IWeightedOccupancyBatchRule
{
    public List<string> Check(WeightedOccupancyBatch item)
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

public sealed record WeightedOccupancyBatchCodePrefixRule(string Prefix) : IWeightedOccupancyBatchRule
{
    public List<string> Check(WeightedOccupancyBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedOccupancyBatchRatioRangeRule(double Min, double Max) : IWeightedOccupancyBatchRule
{
    public List<string> Check(WeightedOccupancyBatch item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted occupancy batch.</summary>
public sealed record WeightedOccupancyBatchValidator(List<IWeightedOccupancyBatchRule> Rules)
{
    public List<string> Validate(WeightedOccupancyBatch item)
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

    public void RequireValid(WeightedOccupancyBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedOccupancyBatch> items)
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

/// <summary>Functions of weighted occupancy batch rules.</summary>
public static class WeightedOccupancyBatchRules
{
    public static WeightedOccupancyBatchValidator DefaultWeightedOccupancyBatchValidator()
    {
        return new WeightedOccupancyBatchValidator(
            new List<IWeightedOccupancyBatchRule>
            {
                new WeightedOccupancyBatchCategoryLengthRule(64),
                new WeightedOccupancyBatchFloorValueRangeRule(9.1, 1485.9),
                new WeightedOccupancyBatchTimezoneLengthRule(255),
                new WeightedOccupancyBatchCeilingRangeRule(2.6, 1420.2),
                new WeightedOccupancyBatchCodePrefixRule("REF"),
                new WeightedOccupancyBatchRatioRangeRule(5.4, 2513.8),
            });
    }
}
