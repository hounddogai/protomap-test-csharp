namespace Acme.Workforce.Compensation.Benchmark.Baseline;

/// <summary>A rule a weighted benchmark baseline must pass. Check returns its problems, or none.</summary>
public interface IWeightedBenchmarkBaselineRule
{
    List<string> Check(WeightedBenchmarkBaseline item);
}

public sealed record WeightedBenchmarkBaselineRequiredCodeRule(string Message) : IWeightedBenchmarkBaselineRule
{
    public List<string> Check(WeightedBenchmarkBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedBenchmarkBaselineRequiredCostCenterRule(string Message) : IWeightedBenchmarkBaselineRule
{
    public List<string> Check(WeightedBenchmarkBaseline item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedBenchmarkBaselineRequiredTitleRule(string Message) : IWeightedBenchmarkBaselineRule
{
    public List<string> Check(WeightedBenchmarkBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedBenchmarkBaselineRatioRangeRule(double Min, double Max) : IWeightedBenchmarkBaselineRule
{
    public List<string> Check(WeightedBenchmarkBaseline item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBenchmarkBaselineCostCenterLengthRule(int MaxLength) : IWeightedBenchmarkBaselineRule
{
    public List<string> Check(WeightedBenchmarkBaseline item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBenchmarkBaselineDurationMinutesRangeRule(int Min, int Max) : IWeightedBenchmarkBaselineRule
{
    public List<string> Check(WeightedBenchmarkBaseline item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a weighted benchmark baseline.</summary>
public sealed record WeightedBenchmarkBaselineValidator(List<IWeightedBenchmarkBaselineRule> Rules)
{
    public List<string> Validate(WeightedBenchmarkBaseline item)
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

    public void RequireValid(WeightedBenchmarkBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedBenchmarkBaseline> items)
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

/// <summary>Functions of weighted benchmark baseline rules.</summary>
public static class WeightedBenchmarkBaselineRules
{
    public static WeightedBenchmarkBaselineValidator DefaultWeightedBenchmarkBaselineValidator()
    {
        return new WeightedBenchmarkBaselineValidator(
            new List<IWeightedBenchmarkBaselineRule>
            {
                new WeightedBenchmarkBaselineRequiredCodeRule("weighted benchmark baseline needs a value"),
                new WeightedBenchmarkBaselineRequiredCostCenterRule("weighted benchmark baseline needs this field"),
                new WeightedBenchmarkBaselineRequiredTitleRule("weighted benchmark baseline needs a value"),
                new WeightedBenchmarkBaselineRatioRangeRule(0.8, 1555.7),
                new WeightedBenchmarkBaselineCostCenterLengthRule(80),
                new WeightedBenchmarkBaselineDurationMinutesRangeRule(64, 80),
            });
    }
}
