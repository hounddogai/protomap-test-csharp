namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate notice calculation.</summary>
public interface IRateNoticeModifier
{
    string Describe();
    double Adjust(RateNotice item, double valueValue);
}

public sealed record TieredRateNoticeAdjuster(List<double> Bounds, List<double> Rates) : IRateNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateNotice item, double valueValue)
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

public sealed record RoundingRateNoticeAdjuster(double Step) : IRateNoticeModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RateNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRateNoticeAdjuster(double Percent) : IRateNoticeModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRateNoticeAdjuster(Registry Flags, string Flag, double Percent) : IRateNoticeModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RateNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRateNoticeAdjuster(double Minimum) : IRateNoticeModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RateNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rate notice steps in order and keeps a trace of each.</summary>
public sealed class RateNoticeEngine(List<IRateNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateNoticeEngine DefaultRateNoticeEngine(Registry flags)
    {
        return new RateNoticeEngine(
            new List<IRateNoticeModifier>
            {
                new TieredRateNoticeAdjuster(new List<double> { 3481.0, 9959.0 }, new List<double> { 0.83, 1.11 }),
                new RoundingRateNoticeAdjuster(0.1),
                new PercentRateNoticeAdjuster(14.5),
                new FlaggedRateNoticeAdjuster(flags, "rate-notice-global", 6.5),
                new MinimumRateNoticeAdjuster(30.2),
            },
            new List<string>(),
            2.4,
            42514.6);
    }

    public double Run(RateNotice item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RateNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
