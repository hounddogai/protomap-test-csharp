namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band rollup calculation.</summary>
public interface IPayBandRollupModifier
{
    string Describe();
    double Adjust(PayBandRollup item, double valueValue);
}

public sealed record FlaggedPayBandRollupAdjuster(Registry Flags, string Flag, double Percent) : IPayBandRollupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPayBandRollupAdjuster(double Percent) : IPayBandRollupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PayBandRollupFteOffsetAdjuster(double Weight) : IPayBandRollupModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(PayBandRollup item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record RoundingPayBandRollupAdjuster(double Step) : IPayBandRollupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitlePayBandRollupAdjuster(Dictionary<string, double> Factors) : IPayBandRollupModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PayBandRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPayBandRollupAdjuster(List<double> Bounds, List<double> Rates) : IPayBandRollupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandRollup item, double valueValue)
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

/// <summary>Runs the pay band rollup steps in order and keeps a trace of each.</summary>
public sealed class PayBandRollupEngine(List<IPayBandRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandRollupEngine DefaultPayBandRollupEngine(Registry flags)
    {
        return new PayBandRollupEngine(
            new List<IPayBandRollupModifier>
            {
                new FlaggedPayBandRollupAdjuster(flags, "pay-band-rollup-weighted", -5.3),
                new PercentPayBandRollupAdjuster(5.9),
                new PayBandRollupFteOffsetAdjuster(1.69),
                new RoundingPayBandRollupAdjuster(1.0),
                new TitlePayBandRollupAdjuster(new Dictionary<string, double> { ["online"] = 1.36, ["east"] = 1.05 }),
                new TieredPayBandRollupAdjuster(new List<double> { 2736.0, 6140.0 }, new List<double> { 1.06, 0.93 }),
            },
            new List<string>(),
            4.2,
            86662.4);
    }

    public double Evaluate(PayBandRollup item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
