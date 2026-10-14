namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool schedule calculation.</summary>
public interface IPoolScheduleFactor
{
    string Describe();
    double Adjust(PoolSchedule item, double valueValue);
}

public sealed record PercentPoolScheduleAdjuster(double Percent) : IPoolScheduleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PoolSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPoolScheduleAdjuster(Registry Flags, string Flag, double Percent) : IPoolScheduleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PoolSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PoolScheduleEndsAtOffsetAdjuster(double Weight) : IPoolScheduleFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(PoolSchedule item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record DepartmentPoolScheduleAdjuster(Dictionary<string, double> Factors) : IPoolScheduleFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PoolSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pool schedule steps in order and keeps a trace of each.</summary>
public sealed class PoolScheduleEngine(List<IPoolScheduleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolScheduleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolScheduleEngine DefaultPoolScheduleEngine(Registry flags)
    {
        return new PoolScheduleEngine(
            new List<IPoolScheduleFactor>
            {
                new PercentPoolScheduleAdjuster(24.2),
                new FlaggedPoolScheduleAdjuster(flags, "pool-schedule-adaptive", 6.6),
                new PoolScheduleEndsAtOffsetAdjuster(1.39),
                new DepartmentPoolScheduleAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.78, ["west"] = 0.87 }),
            },
            new List<string>(),
            0.8,
            92933.1);
    }

    public double Compute(PoolSchedule item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PoolSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
