namespace Acme.Workforce.Compensation.Benchmark.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered benchmark summary calculation.</summary>
public interface ITieredBenchmarkSummaryFactor
{
    string Describe();
    double Adjust(TieredBenchmarkSummary item, double valueValue);
}

public sealed record NameTieredBenchmarkSummaryAdjuster(Dictionary<string, double> Factors) : ITieredBenchmarkSummaryFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TieredBenchmarkSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTieredBenchmarkSummaryAdjuster(double Step) : ITieredBenchmarkSummaryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredBenchmarkSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTieredBenchmarkSummaryAdjuster(Registry Flags, string Flag, double Percent) : ITieredBenchmarkSummaryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredBenchmarkSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTieredBenchmarkSummaryAdjuster(double Minimum) : ITieredBenchmarkSummaryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredBenchmarkSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tiered benchmark summary steps in order and keeps a trace of each.</summary>
public sealed class TieredBenchmarkSummaryEngine(List<ITieredBenchmarkSummaryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredBenchmarkSummaryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredBenchmarkSummaryEngine DefaultTieredBenchmarkSummaryEngine(Registry flags)
    {
        return new TieredBenchmarkSummaryEngine(
            new List<ITieredBenchmarkSummaryFactor>
            {
                new NameTieredBenchmarkSummaryAdjuster(new Dictionary<string, double> { ["online"] = 0.79 }),
                new RoundingTieredBenchmarkSummaryAdjuster(0.25),
                new FlaggedTieredBenchmarkSummaryAdjuster(flags, "tiered-benchmark-summary-pending", 4.5),
                new MinimumTieredBenchmarkSummaryAdjuster(34.6),
            },
            new List<string>(),
            4.9,
            11406.8);
    }

    public double Price(TieredBenchmarkSummary item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredBenchmarkSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
