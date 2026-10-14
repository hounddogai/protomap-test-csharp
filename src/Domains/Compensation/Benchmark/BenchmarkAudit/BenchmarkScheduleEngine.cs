namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark schedule calculation.</summary>
public interface IBenchmarkScheduleAdjuster
{
    string Describe();
    double Adjust(BenchmarkSchedule item, double valueValue);
}

public sealed record FlaggedBenchmarkScheduleAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBenchmarkScheduleAdjuster(double Minimum) : IBenchmarkScheduleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BenchmarkSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ChannelBenchmarkScheduleAdjuster(Dictionary<string, double> Factors) : IBenchmarkScheduleAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(BenchmarkSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingBenchmarkScheduleAdjuster(double Step) : IBenchmarkScheduleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BenchmarkSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBenchmarkScheduleAdjuster(double Percent) : IBenchmarkScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the benchmark schedule steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkScheduleEngine(List<IBenchmarkScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkScheduleEngine DefaultBenchmarkScheduleEngine(Registry flags)
    {
        return new BenchmarkScheduleEngine(
            new List<IBenchmarkScheduleAdjuster>
            {
                new FlaggedBenchmarkScheduleAdjuster(flags, "benchmark-schedule-scheduled", -2.6),
                new MinimumBenchmarkScheduleAdjuster(29.9),
                new ChannelBenchmarkScheduleAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.28, ["west"] = 1.02 }),
                new RoundingBenchmarkScheduleAdjuster(0.05),
                new PercentBenchmarkScheduleAdjuster(-4.3),
            },
            new List<string>(),
            0.8,
            30571.8);
    }

    public double Compute(BenchmarkSchedule item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
