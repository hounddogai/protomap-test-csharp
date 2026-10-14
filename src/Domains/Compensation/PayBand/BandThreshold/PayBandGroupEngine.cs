namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pay band group calculation.</summary>
public interface IPayBandGroupModifier
{
    string Describe();
    double Adjust(PayBandGroup item, double valueValue);
}

public sealed record FlaggedPayBandGroupAdjuster(Registry Flags, string Flag, double Percent) : IPayBandGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPayBandGroupAdjuster(double Percent) : IPayBandGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPayBandGroupAdjuster(List<double> Bounds, List<double> Rates) : IPayBandGroupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandGroup item, double valueValue)
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

/// <summary>Runs the pay band group steps in order and keeps a trace of each.</summary>
public sealed class PayBandGroupEngine(List<IPayBandGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandGroupEngine DefaultPayBandGroupEngine(Registry flags)
    {
        return new PayBandGroupEngine(
            new List<IPayBandGroupModifier>
            {
                new FlaggedPayBandGroupAdjuster(flags, "pay-band-group-capped", 0.2),
                new PercentPayBandGroupAdjuster(11.7),
                new TieredPayBandGroupAdjuster(new List<double> { 8467.0, 9193.0 }, new List<double> { 1.15, 0.97 }),
            },
            new List<string>(),
            4.9,
            95910.7);
    }

    public double Price(PayBandGroup item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
