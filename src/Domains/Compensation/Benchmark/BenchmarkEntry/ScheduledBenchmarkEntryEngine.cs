namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the scheduled benchmark entry calculation.</summary>
public interface IScheduledBenchmarkEntryAdjuster
{
    string Describe();
    double Adjust(ScheduledBenchmarkEntry item, double valueValue);
}

public sealed record LabelScheduledBenchmarkEntryAdjuster(Dictionary<string, double> Factors) : IScheduledBenchmarkEntryAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ScheduledBenchmarkEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedScheduledBenchmarkEntryAdjuster(Registry Flags, string Flag, double Percent) : IScheduledBenchmarkEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledBenchmarkEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentScheduledBenchmarkEntryAdjuster(double Percent) : IScheduledBenchmarkEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledBenchmarkEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled benchmark entry steps in order and keeps a trace of each.</summary>
public sealed class ScheduledBenchmarkEntryEngine(List<IScheduledBenchmarkEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledBenchmarkEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledBenchmarkEntryEngine DefaultScheduledBenchmarkEntryEngine(Registry flags)
    {
        return new ScheduledBenchmarkEntryEngine(
            new List<IScheduledBenchmarkEntryAdjuster>
            {
                new LabelScheduledBenchmarkEntryAdjuster(new Dictionary<string, double> { ["north"] = 1.4 }),
                new FlaggedScheduledBenchmarkEntryAdjuster(flags, "scheduled-benchmark-entry-capped", 2.9),
                new PercentScheduledBenchmarkEntryAdjuster(-16.4),
            },
            new List<string>(),
            0.1,
            89017.5);
    }

    public double Price(ScheduledBenchmarkEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledBenchmarkEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
