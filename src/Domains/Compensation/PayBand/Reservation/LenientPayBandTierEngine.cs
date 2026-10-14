namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient pay band tier calculation.</summary>
public interface ILenientPayBandTierModifier
{
    string Describe();
    double Adjust(LenientPayBandTier item, double valueValue);
}

public sealed record RoundingLenientPayBandTierAdjuster(double Step) : ILenientPayBandTierModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientPayBandTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLenientPayBandTierAdjuster(double Cap) : ILenientPayBandTierModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientPayBandTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLenientPayBandTierAdjuster(Registry Flags, string Flag, double Percent) : ILenientPayBandTierModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientPayBandTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientPayBandTierAdjuster(double Minimum) : ILenientPayBandTierModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientPayBandTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the lenient pay band tier steps in order and keeps a trace of each.</summary>
public sealed class LenientPayBandTierEngine(List<ILenientPayBandTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientPayBandTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientPayBandTierEngine DefaultLenientPayBandTierEngine(Registry flags)
    {
        return new LenientPayBandTierEngine(
            new List<ILenientPayBandTierModifier>
            {
                new RoundingLenientPayBandTierAdjuster(0.5),
                new CapLenientPayBandTierAdjuster(37542.0),
                new FlaggedLenientPayBandTierAdjuster(flags, "lenient-pay-band-tier-manual", 3.0),
                new MinimumLenientPayBandTierAdjuster(37.4),
            },
            new List<string>(),
            0.8,
            51668.2);
    }

    public double Evaluate(LenientPayBandTier item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientPayBandTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
