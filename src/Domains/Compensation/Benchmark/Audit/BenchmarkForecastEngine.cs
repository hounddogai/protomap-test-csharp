namespace Acme.Workforce.Compensation.Benchmark.Audit;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the benchmark forecast calculation.</summary>
public interface IBenchmarkForecastModifier
{
    string Describe();
    double Adjust(BenchmarkForecast item, double valueValue);
}

public sealed record CapBenchmarkForecastAdjuster(double Cap) : IBenchmarkForecastModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BenchmarkForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelBenchmarkForecastAdjuster(Dictionary<string, double> Factors) : IBenchmarkForecastModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BenchmarkForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentBenchmarkForecastAdjuster(double Percent) : IBenchmarkForecastModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBenchmarkForecastAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkForecastModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBenchmarkForecastAdjuster(List<double> Bounds, List<double> Rates) : IBenchmarkForecastModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BenchmarkForecast item, double valueValue)
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

/// <summary>Runs the benchmark forecast steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkForecastEngine(List<IBenchmarkForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkForecastEngine DefaultBenchmarkForecastEngine(Registry flags)
    {
        return new BenchmarkForecastEngine(
            new List<IBenchmarkForecastModifier>
            {
                new CapBenchmarkForecastAdjuster(20144.0),
                new LabelBenchmarkForecastAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.26, ["online"] = 0.95, ["retail"] = 0.83 }),
                new PercentBenchmarkForecastAdjuster(23.1),
                new FlaggedBenchmarkForecastAdjuster(flags, "benchmark-forecast-default", 3.1),
                new TieredBenchmarkForecastAdjuster(
                    new List<double> { 2565.0, 5940.0 },
                    new List<double> { 1.18, 0.84 }),
            },
            new List<string>(),
            3.4,
            80372.8);
    }

    public double Evaluate(BenchmarkForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
