namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg threshold calculation.</summary>
public interface IReorgThresholdAdjuster
{
    string Describe();
    double Adjust(ReorgThreshold item, double valueValue);
}

public sealed record PercentReorgThresholdAdjuster(double Percent) : IReorgThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReorgThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingReorgThresholdAdjuster(double Step) : IReorgThresholdAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ReorgThresholdCostOffsetAdjuster(double Weight) : IReorgThresholdAdjuster
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(ReorgThreshold item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record MinimumReorgThresholdAdjuster(double Minimum) : IReorgThresholdAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedReorgThresholdAdjuster(Registry Flags, string Flag, double Percent) : IReorgThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the reorg threshold steps in order and keeps a trace of each.</summary>
public sealed class ReorgThresholdEngine(List<IReorgThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgThresholdEngine DefaultReorgThresholdEngine(Registry flags)
    {
        return new ReorgThresholdEngine(
            new List<IReorgThresholdAdjuster>
            {
                new PercentReorgThresholdAdjuster(-0.5),
                new RoundingReorgThresholdAdjuster(5.0),
                new ReorgThresholdCostOffsetAdjuster(0.65),
                new MinimumReorgThresholdAdjuster(0.9),
                new FlaggedReorgThresholdAdjuster(flags, "reorg-threshold-lenient", 6.1),
            },
            new List<string>(),
            0.1,
            10857.5);
    }

    public double Price(ReorgThreshold item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
