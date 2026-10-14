namespace Acme.Workforce.Compensation.PayBand.Exception;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tiered pay band draft calculation.</summary>
public interface ITieredPayBandDraftAdjuster
{
    string Describe();
    double Adjust(TieredPayBandDraft item, double valueValue);
}

public sealed record ChannelTieredPayBandDraftAdjuster(Dictionary<string, double> Factors) : ITieredPayBandDraftAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(TieredPayBandDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredTieredPayBandDraftAdjuster(List<double> Bounds, List<double> Rates) : ITieredPayBandDraftAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredPayBandDraft item, double valueValue)
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

public sealed record CapTieredPayBandDraftAdjuster(double Cap) : ITieredPayBandDraftAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredPayBandDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedTieredPayBandDraftAdjuster(Registry Flags, string Flag, double Percent) : ITieredPayBandDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredPayBandDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered pay band draft steps in order and keeps a trace of each.</summary>
public sealed class TieredPayBandDraftEngine(List<ITieredPayBandDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredPayBandDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredPayBandDraftEngine DefaultTieredPayBandDraftEngine(Registry flags)
    {
        return new TieredPayBandDraftEngine(
            new List<ITieredPayBandDraftAdjuster>
            {
                new ChannelTieredPayBandDraftAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.84, ["online"] = 1.18 }),
                new TieredTieredPayBandDraftAdjuster(
                    new List<double> { 405.0, 7524.0, 9532.0 },
                    new List<double> { 1.19, 0.81, 1.11 }),
                new CapTieredPayBandDraftAdjuster(24450.0),
                new FlaggedTieredPayBandDraftAdjuster(flags, "tiered-pay-band-draft-lenient", -2.5),
            },
            new List<string>(),
            3.3,
            21577.0);
    }

    public double Compute(TieredPayBandDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<TieredPayBandDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
