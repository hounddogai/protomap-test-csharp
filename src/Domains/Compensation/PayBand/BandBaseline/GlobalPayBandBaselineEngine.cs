namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global pay band baseline calculation.</summary>
public interface IGlobalPayBandBaselineAdjuster
{
    string Describe();
    double Adjust(GlobalPayBandBaseline item, double valueValue);
}

public sealed record CapGlobalPayBandBaselineAdjuster(double Cap) : IGlobalPayBandBaselineAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalPayBandBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedGlobalPayBandBaselineAdjuster(Registry Flags, string Flag, double Percent) : IGlobalPayBandBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalPayBandBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameGlobalPayBandBaselineAdjuster(Dictionary<string, double> Factors) : IGlobalPayBandBaselineAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(GlobalPayBandBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingGlobalPayBandBaselineAdjuster(double Step) : IGlobalPayBandBaselineAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalPayBandBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record GlobalPayBandBaselineUpdatedAtOffsetAdjuster(double Weight) : IGlobalPayBandBaselineAdjuster
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(GlobalPayBandBaseline item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

/// <summary>Runs the global pay band baseline steps in order and keeps a trace of each.</summary>
public sealed class GlobalPayBandBaselineEngine(List<IGlobalPayBandBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPayBandBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPayBandBaselineEngine DefaultGlobalPayBandBaselineEngine(Registry flags)
    {
        return new GlobalPayBandBaselineEngine(
            new List<IGlobalPayBandBaselineAdjuster>
            {
                new CapGlobalPayBandBaselineAdjuster(31115.0),
                new FlaggedGlobalPayBandBaselineAdjuster(flags, "global-pay-band-baseline-strict", -6.0),
                new NameGlobalPayBandBaselineAdjuster(new Dictionary<string, double> { ["south"] = 1.07 }),
                new RoundingGlobalPayBandBaselineAdjuster(0.1),
                new GlobalPayBandBaselineUpdatedAtOffsetAdjuster(1.79),
            },
            new List<string>(),
            3.0,
            38043.9);
    }

    public double Run(GlobalPayBandBaseline item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalPayBandBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
