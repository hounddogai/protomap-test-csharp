namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive pay band group calculation.</summary>
public interface IAdaptivePayBandGroupFactor
{
    string Describe();
    double Adjust(AdaptivePayBandGroup item, double valueValue);
}

public sealed record FlaggedAdaptivePayBandGroupAdjuster(Registry Flags, string Flag, double Percent) : IAdaptivePayBandGroupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptivePayBandGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAdaptivePayBandGroupAdjuster(double Step) : IAdaptivePayBandGroupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptivePayBandGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionAdaptivePayBandGroupAdjuster(Dictionary<string, double> Factors) : IAdaptivePayBandGroupFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(AdaptivePayBandGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive pay band group steps in order and keeps a trace of each.</summary>
public sealed class AdaptivePayBandGroupEngine(List<IAdaptivePayBandGroupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptivePayBandGroupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptivePayBandGroupEngine DefaultAdaptivePayBandGroupEngine(Registry flags)
    {
        return new AdaptivePayBandGroupEngine(
            new List<IAdaptivePayBandGroupFactor>
            {
                new FlaggedAdaptivePayBandGroupAdjuster(flags, "adaptive-pay-band-group-weighted", 9.3),
                new RoundingAdaptivePayBandGroupAdjuster(0.1),
                new RegionAdaptivePayBandGroupAdjuster(new Dictionary<string, double> { ["north"] = 1.34 }),
            },
            new List<string>(),
            2.9,
            54671.2);
    }

    public double Compute(AdaptivePayBandGroup item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptivePayBandGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
