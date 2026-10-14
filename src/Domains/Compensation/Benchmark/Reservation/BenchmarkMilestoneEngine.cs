namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the benchmark milestone calculation.</summary>
public interface IBenchmarkMilestoneStep
{
    string Describe();
    double Adjust(BenchmarkMilestone item, double valueValue);
}

public sealed record TieredBenchmarkMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IBenchmarkMilestoneStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BenchmarkMilestone item, double valueValue)
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

public sealed record LabelBenchmarkMilestoneAdjuster(Dictionary<string, double> Factors) : IBenchmarkMilestoneStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BenchmarkMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBenchmarkMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkMilestoneStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the benchmark milestone steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkMilestoneEngine(List<IBenchmarkMilestoneStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkMilestoneStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkMilestoneEngine DefaultBenchmarkMilestoneEngine(Registry flags)
    {
        return new BenchmarkMilestoneEngine(
            new List<IBenchmarkMilestoneStep>
            {
                new TieredBenchmarkMilestoneAdjuster(
                    new List<double> { 4200.0, 5651.0, 5916.0, 9495.0 },
                    new List<double> { 0.96, 1.01, 0.83, 1.17 }),
                new LabelBenchmarkMilestoneAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.18, ["central"] = 0.91, ["retail"] = 1.34 }),
                new FlaggedBenchmarkMilestoneAdjuster(flags, "benchmark-milestone-batched", -5.5),
            },
            new List<string>(),
            2.3,
            89338.4);
    }

    public double Compute(BenchmarkMilestone item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
