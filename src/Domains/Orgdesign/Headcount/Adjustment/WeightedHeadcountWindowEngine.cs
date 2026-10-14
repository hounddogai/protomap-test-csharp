namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted headcount window calculation.</summary>
public interface IWeightedHeadcountWindowAdjuster
{
    string Describe();
    double Adjust(WeightedHeadcountWindow item, double valueValue);
}

public sealed record FlaggedWeightedHeadcountWindowAdjuster(Registry Flags, string Flag, double Percent) : IWeightedHeadcountWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedHeadcountWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWeightedHeadcountWindowAdjuster(double Step) : IWeightedHeadcountWindowAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedHeadcountWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record WeightedHeadcountWindowAttemptsOffsetAdjuster(double Weight) : IWeightedHeadcountWindowAdjuster
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(WeightedHeadcountWindow item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record CapWeightedHeadcountWindowAdjuster(double Cap) : IWeightedHeadcountWindowAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedHeadcountWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ChannelWeightedHeadcountWindowAdjuster(Dictionary<string, double> Factors) : IWeightedHeadcountWindowAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(WeightedHeadcountWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the weighted headcount window steps in order and keeps a trace of each.</summary>
public sealed class WeightedHeadcountWindowEngine(List<IWeightedHeadcountWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedHeadcountWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedHeadcountWindowEngine DefaultWeightedHeadcountWindowEngine(Registry flags)
    {
        return new WeightedHeadcountWindowEngine(
            new List<IWeightedHeadcountWindowAdjuster>
            {
                new FlaggedWeightedHeadcountWindowAdjuster(flags, "weighted-headcount-window-rolling", 4.7),
                new RoundingWeightedHeadcountWindowAdjuster(0.25),
                new WeightedHeadcountWindowAttemptsOffsetAdjuster(1.3),
                new CapWeightedHeadcountWindowAdjuster(24006.0),
                new ChannelWeightedHeadcountWindowAdjuster(new Dictionary<string, double> { ["south"] = 1.27 }),
            },
            new List<string>(),
            2.8,
            34245.7);
    }

    public double Evaluate(WeightedHeadcountWindow item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedHeadcountWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
