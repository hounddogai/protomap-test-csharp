namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark batch calculation.</summary>
public interface IBenchmarkBatchStep
{
    string Describe();
    double Adjust(BenchmarkBatch item, double valueValue);
}

public sealed record PercentBenchmarkBatchAdjuster(double Percent) : IBenchmarkBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelBenchmarkBatchAdjuster(Dictionary<string, double> Factors) : IBenchmarkBatchStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BenchmarkBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredBenchmarkBatchAdjuster(List<double> Bounds, List<double> Rates) : IBenchmarkBatchStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BenchmarkBatch item, double valueValue)
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

public sealed record RoundingBenchmarkBatchAdjuster(double Step) : IBenchmarkBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BenchmarkBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapBenchmarkBatchAdjuster(double Cap) : IBenchmarkBatchStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BenchmarkBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the benchmark batch steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkBatchEngine(List<IBenchmarkBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkBatchEngine DefaultBenchmarkBatchEngine()
    {
        return new BenchmarkBatchEngine(
            new List<IBenchmarkBatchStep>
            {
                new PercentBenchmarkBatchAdjuster(18.5),
                new LabelBenchmarkBatchAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.4, ["north"] = 1.22, ["retail"] = 0.94 }),
                new TieredBenchmarkBatchAdjuster(
                    new List<double> { 5520.0, 5627.0, 8277.0, 8595.0 },
                    new List<double> { 0.84, 1.15, 1.08, 1.07 }),
                new RoundingBenchmarkBatchAdjuster(0.25),
                new CapBenchmarkBatchAdjuster(23228.0),
            },
            new List<string>(),
            2.2,
            98693.7);
    }

    public double Price(BenchmarkBatch item)
    {
        var current = item.ProjectedTotal();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<BenchmarkBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
