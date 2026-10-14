namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment tier calculation.</summary>
public interface IAdjustmentTierAdjuster
{
    string Describe();
    double Adjust(AdjustmentTier item, double valueValue);
}

public sealed record PercentAdjustmentTierAdjuster(double Percent) : IAdjustmentTierAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAdjustmentTierAdjuster(List<double> Bounds, List<double> Rates) : IAdjustmentTierAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdjustmentTier item, double valueValue)
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

public sealed record FlaggedAdjustmentTierAdjuster(Registry Flags, string Flag, double Percent) : IAdjustmentTierAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdjustmentTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdjustmentTierAdjuster(double Cap) : IAdjustmentTierAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LocaleAdjustmentTierAdjuster(Dictionary<string, double> Factors) : IAdjustmentTierAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(AdjustmentTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AdjustmentTierWindowDaysOffsetAdjuster(double Weight) : IAdjustmentTierAdjuster
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(AdjustmentTier item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

/// <summary>Runs the adjustment tier steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentTierEngine(List<IAdjustmentTierAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentTierAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentTierEngine DefaultAdjustmentTierEngine(Registry flags)
    {
        return new AdjustmentTierEngine(
            new List<IAdjustmentTierAdjuster>
            {
                new PercentAdjustmentTierAdjuster(2.8),
                new TieredAdjustmentTierAdjuster(
                    new List<double> { 1061.0, 9491.0 },
                    new List<double> { 1.03, 1.19 }),
                new FlaggedAdjustmentTierAdjuster(flags, "adjustment-tier-weighted", -3.1),
                new CapAdjustmentTierAdjuster(29319.0),
                new LocaleAdjustmentTierAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.86, ["central"] = 0.85, ["retail"] = 0.7 }),
                new AdjustmentTierWindowDaysOffsetAdjuster(0.86),
            },
            new List<string>(),
            0.5,
            8580.5);
    }

    public double Price(AdjustmentTier item)
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
