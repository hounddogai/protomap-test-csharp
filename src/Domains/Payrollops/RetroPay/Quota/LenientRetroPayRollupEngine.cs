namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient retro pay rollup calculation.</summary>
public interface ILenientRetroPayRollupFactor
{
    string Describe();
    double Adjust(LenientRetroPayRollup item, double valueValue);
}

public sealed record RoundingLenientRetroPayRollupAdjuster(double Step) : ILenientRetroPayRollupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientRetroPayRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLenientRetroPayRollupAdjuster(Registry Flags, string Flag, double Percent) : ILenientRetroPayRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientRetroPayRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientRetroPayRollupFteOffsetAdjuster(double Weight) : ILenientRetroPayRollupFactor
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(LenientRetroPayRollup item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record MinimumLenientRetroPayRollupAdjuster(double Minimum) : ILenientRetroPayRollupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientRetroPayRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLenientRetroPayRollupAdjuster(List<double> Bounds, List<double> Rates) : ILenientRetroPayRollupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientRetroPayRollup item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record PercentLenientRetroPayRollupAdjuster(double Percent) : ILenientRetroPayRollupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientRetroPayRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient retro pay rollup steps in order and keeps a trace of each.</summary>
public sealed class LenientRetroPayRollupEngine(List<ILenientRetroPayRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientRetroPayRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientRetroPayRollupEngine DefaultLenientRetroPayRollupEngine(Registry flags)
    {
        return new LenientRetroPayRollupEngine(
            new List<ILenientRetroPayRollupFactor>
            {
                new RoundingLenientRetroPayRollupAdjuster(1.0),
                new FlaggedLenientRetroPayRollupAdjuster(flags, "lenient-retro-pay-rollup-manual", -4.3),
                new LenientRetroPayRollupFteOffsetAdjuster(1.43),
                new MinimumLenientRetroPayRollupAdjuster(7.7),
                new TieredLenientRetroPayRollupAdjuster(
                    new List<double> { 823.0, 2329.0, 8182.0, 8869.0 },
                    new List<double> { 0.97, 1.11, 0.99, 0.98 }),
                new PercentLenientRetroPayRollupAdjuster(6.8),
            },
            new List<string>(),
            2.3,
            19541.4);
    }

    public double Compute(LenientRetroPayRollup item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientRetroPayRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
