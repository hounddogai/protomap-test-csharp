namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark allocation calculation.</summary>
public interface IBenchmarkAllocationStep
{
    string Describe();
    double Adjust(BenchmarkAllocation item, double valueValue);
}

public sealed record TieredBenchmarkAllocationAdjuster(List<double> Bounds, List<double> Rates) : IBenchmarkAllocationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BenchmarkAllocation item, double valueValue)
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

public sealed record CountryBenchmarkAllocationAdjuster(Dictionary<string, double> Factors) : IBenchmarkAllocationStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(BenchmarkAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingBenchmarkAllocationAdjuster(double Step) : IBenchmarkAllocationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BenchmarkAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumBenchmarkAllocationAdjuster(double Minimum) : IBenchmarkAllocationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BenchmarkAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBenchmarkAllocationAdjuster(double Percent) : IBenchmarkAllocationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BenchmarkAllocationEndsAtOffsetAdjuster(double Weight) : IBenchmarkAllocationStep
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(BenchmarkAllocation item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the benchmark allocation steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkAllocationEngine(List<IBenchmarkAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkAllocationEngine DefaultBenchmarkAllocationEngine()
    {
        return new BenchmarkAllocationEngine(
            new List<IBenchmarkAllocationStep>
            {
                new TieredBenchmarkAllocationAdjuster(
                    new List<double> { 2984.0, 6370.0 },
                    new List<double> { 1.16, 1.16 }),
                new CountryBenchmarkAllocationAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.0, ["central"] = 0.94 }),
                new RoundingBenchmarkAllocationAdjuster(1.0),
                new MinimumBenchmarkAllocationAdjuster(33.3),
                new PercentBenchmarkAllocationAdjuster(-20.4),
                new BenchmarkAllocationEndsAtOffsetAdjuster(1.93),
            },
            new List<string>(),
            3.2,
            42911.9);
    }

    public double Evaluate(BenchmarkAllocation item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
