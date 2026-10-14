namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark estimate calculation.</summary>
public interface IBenchmarkEstimateModifier
{
    string Describe();
    double Adjust(BenchmarkEstimate item, double valueValue);
}

public sealed record TitleBenchmarkEstimateAdjuster(Dictionary<string, double> Factors) : IBenchmarkEstimateModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BenchmarkEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingBenchmarkEstimateAdjuster(double Step) : IBenchmarkEstimateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BenchmarkEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredBenchmarkEstimateAdjuster(List<double> Bounds, List<double> Rates) : IBenchmarkEstimateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BenchmarkEstimate item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

/// <summary>Runs the benchmark estimate steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkEstimateEngine(List<IBenchmarkEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkEstimateEngine DefaultBenchmarkEstimateEngine()
    {
        return new BenchmarkEstimateEngine(
            new List<IBenchmarkEstimateModifier>
            {
                new TitleBenchmarkEstimateAdjuster(new Dictionary<string, double> { ["north"] = 0.8 }),
                new RoundingBenchmarkEstimateAdjuster(5.0),
                new TieredBenchmarkEstimateAdjuster(
                    new List<double> { 582.0, 5032.0, 7496.0, 8285.0 },
                    new List<double> { 1.09, 1.07, 1.03, 0.88 }),
            },
            new List<string>(),
            3.7,
            36719.7);
    }

    public double Evaluate(BenchmarkEstimate item)
    {
        var current = item.WeightedTotal();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<BenchmarkEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
