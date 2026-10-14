namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark checkpoint calculation.</summary>
public interface IBenchmarkCheckpointFactor
{
    string Describe();
    double Adjust(BenchmarkCheckpoint item, double valueValue);
}

public sealed record PercentBenchmarkCheckpointAdjuster(double Percent) : IBenchmarkCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBenchmarkCheckpointAdjuster(double Minimum) : IBenchmarkCheckpointFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BenchmarkCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredBenchmarkCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IBenchmarkCheckpointFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BenchmarkCheckpoint item, double valueValue)
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

public sealed record BenchmarkCheckpointBatchSizeOffsetAdjuster(double Weight) : IBenchmarkCheckpointFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(BenchmarkCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

/// <summary>Runs the benchmark checkpoint steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkCheckpointEngine(List<IBenchmarkCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkCheckpointEngine DefaultBenchmarkCheckpointEngine()
    {
        return new BenchmarkCheckpointEngine(
            new List<IBenchmarkCheckpointFactor>
            {
                new PercentBenchmarkCheckpointAdjuster(-2.9),
                new MinimumBenchmarkCheckpointAdjuster(43.6),
                new TieredBenchmarkCheckpointAdjuster(
                    new List<double> { 4285.0, 4710.0 },
                    new List<double> { 0.84, 0.8 }),
                new BenchmarkCheckpointBatchSizeOffsetAdjuster(0.22),
            },
            new List<string>(),
            3.4,
            86746.6);
    }

    public double Price(BenchmarkCheckpoint item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
