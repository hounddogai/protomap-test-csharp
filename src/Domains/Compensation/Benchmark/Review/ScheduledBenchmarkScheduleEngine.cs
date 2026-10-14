namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled benchmark schedule calculation.</summary>
public interface IScheduledBenchmarkScheduleFactor
{
    string Describe();
    double Adjust(ScheduledBenchmarkSchedule item, double valueValue);
}

public sealed record MinimumScheduledBenchmarkScheduleAdjuster(double Minimum) : IScheduledBenchmarkScheduleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledBenchmarkSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentScheduledBenchmarkScheduleAdjuster(double Percent) : IScheduledBenchmarkScheduleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledBenchmarkSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelScheduledBenchmarkScheduleAdjuster(Dictionary<string, double> Factors) : IScheduledBenchmarkScheduleFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ScheduledBenchmarkSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scheduled benchmark schedule steps in order and keeps a trace of each.</summary>
public sealed class ScheduledBenchmarkScheduleEngine(List<IScheduledBenchmarkScheduleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledBenchmarkScheduleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledBenchmarkScheduleEngine DefaultScheduledBenchmarkScheduleEngine()
    {
        return new ScheduledBenchmarkScheduleEngine(
            new List<IScheduledBenchmarkScheduleFactor>
            {
                new MinimumScheduledBenchmarkScheduleAdjuster(27.9),
                new PercentScheduledBenchmarkScheduleAdjuster(13.7),
                new LabelScheduledBenchmarkScheduleAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.09, ["retail"] = 0.88 }),
            },
            new List<string>(),
            3.2,
            23285.6);
    }

    public double Run(ScheduledBenchmarkSchedule item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledBenchmarkSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
