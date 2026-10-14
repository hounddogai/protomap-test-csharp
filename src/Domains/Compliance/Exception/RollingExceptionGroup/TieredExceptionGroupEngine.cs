namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered exception group calculation.</summary>
public interface ITieredExceptionGroupAdjuster
{
    string Describe();
    double Adjust(TieredExceptionGroup item, double valueValue);
}

public sealed record MinimumTieredExceptionGroupAdjuster(double Minimum) : ITieredExceptionGroupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredExceptionGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ChannelTieredExceptionGroupAdjuster(Dictionary<string, double> Factors) : ITieredExceptionGroupAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(TieredExceptionGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentTieredExceptionGroupAdjuster(double Percent) : ITieredExceptionGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredExceptionGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedTieredExceptionGroupAdjuster(Registry Flags, string Flag, double Percent) : ITieredExceptionGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredExceptionGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredExceptionGroupRevisionOffsetAdjuster(double Weight) : ITieredExceptionGroupAdjuster
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(TieredExceptionGroup item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record RoundingTieredExceptionGroupAdjuster(double Step) : ITieredExceptionGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredExceptionGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tiered exception group steps in order and keeps a trace of each.</summary>
public sealed class TieredExceptionGroupEngine(List<ITieredExceptionGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredExceptionGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredExceptionGroupEngine DefaultTieredExceptionGroupEngine(Registry flags)
    {
        return new TieredExceptionGroupEngine(
            new List<ITieredExceptionGroupAdjuster>
            {
                new MinimumTieredExceptionGroupAdjuster(13.9),
                new ChannelTieredExceptionGroupAdjuster(new Dictionary<string, double> { ["east"] = 1.09 }),
                new PercentTieredExceptionGroupAdjuster(13.6),
                new FlaggedTieredExceptionGroupAdjuster(flags, "tiered-exception-group-rolling", 0.9),
                new TieredExceptionGroupRevisionOffsetAdjuster(1.72),
                new RoundingTieredExceptionGroupAdjuster(0.1),
            },
            new List<string>(),
            4.7,
            18354.8);
    }

    public double Run(TieredExceptionGroup item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredExceptionGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
