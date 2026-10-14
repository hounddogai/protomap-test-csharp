namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool notice calculation.</summary>
public interface IPoolNoticeStep
{
    string Describe();
    double Adjust(PoolNotice item, double valueValue);
}

public sealed record FlaggedPoolNoticeAdjuster(Registry Flags, string Flag, double Percent) : IPoolNoticeStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PoolNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPoolNoticeAdjuster(double Minimum) : IPoolNoticeStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SegmentPoolNoticeAdjuster(Dictionary<string, double> Factors) : IPoolNoticeStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(PoolNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPoolNoticeAdjuster(double Cap) : IPoolNoticeStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PoolNoticeFloorValueOffsetAdjuster(double Weight) : IPoolNoticeStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(PoolNotice item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

/// <summary>Runs the pool notice steps in order and keeps a trace of each.</summary>
public sealed class PoolNoticeEngine(List<IPoolNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolNoticeEngine DefaultPoolNoticeEngine(Registry flags)
    {
        return new PoolNoticeEngine(
            new List<IPoolNoticeStep>
            {
                new FlaggedPoolNoticeAdjuster(flags, "pool-notice-strict", 8.5),
                new MinimumPoolNoticeAdjuster(38.7),
                new SegmentPoolNoticeAdjuster(new Dictionary<string, double> { ["west"] = 1.12 }),
                new CapPoolNoticeAdjuster(30963.0),
                new PoolNoticeFloorValueOffsetAdjuster(0.44),
            },
            new List<string>(),
            2.4,
            49186.2);
    }

    public double Price(PoolNotice item)
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

    public Dictionary<string, double> EvaluateAll(List<PoolNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
