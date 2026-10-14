namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate override calculation.</summary>
public interface IRateOverrideFactor
{
    string Describe();
    double Adjust(RateOverride item, double valueValue);
}

public sealed record PercentRateOverrideAdjuster(double Percent) : IRateOverrideFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleRateOverrideAdjuster(Dictionary<string, double> Factors) : IRateOverrideFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(RateOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRateOverrideAdjuster(Registry Flags, string Flag, double Percent) : IRateOverrideFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RateOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRateOverrideAdjuster(double Cap) : IRateOverrideFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RateOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rate override steps in order and keeps a trace of each.</summary>
public sealed class RateOverrideEngine(List<IRateOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateOverrideEngine DefaultRateOverrideEngine(Registry flags)
    {
        return new RateOverrideEngine(
            new List<IRateOverrideFactor>
            {
                new PercentRateOverrideAdjuster(9.5),
                new LocaleRateOverrideAdjuster(new Dictionary<string, double> { ["north"] = 1.26 }),
                new FlaggedRateOverrideAdjuster(flags, "rate-override-weighted", 4.4),
                new CapRateOverrideAdjuster(45075.0),
            },
            new List<string>(),
            2.4,
            95609.1);
    }

    public double Price(RateOverride item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RateOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
