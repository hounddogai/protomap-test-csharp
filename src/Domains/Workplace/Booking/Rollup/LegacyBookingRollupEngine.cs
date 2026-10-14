namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy booking rollup calculation.</summary>
public interface ILegacyBookingRollupModifier
{
    string Describe();
    double Adjust(LegacyBookingRollup item, double valueValue);
}

public sealed record LegacyBookingRollupCreatedAtOffsetAdjuster(double Weight) : ILegacyBookingRollupModifier
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(LegacyBookingRollup item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record TitleLegacyBookingRollupAdjuster(Dictionary<string, double> Factors) : ILegacyBookingRollupModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LegacyBookingRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLegacyBookingRollupAdjuster(List<double> Bounds, List<double> Rates) : ILegacyBookingRollupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyBookingRollup item, double valueValue)
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

public sealed record FlaggedLegacyBookingRollupAdjuster(Registry Flags, string Flag, double Percent) : ILegacyBookingRollupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyBookingRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyBookingRollupAdjuster(double Minimum) : ILegacyBookingRollupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyBookingRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy booking rollup steps in order and keeps a trace of each.</summary>
public sealed class LegacyBookingRollupEngine(List<ILegacyBookingRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyBookingRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyBookingRollupEngine DefaultLegacyBookingRollupEngine(Registry flags)
    {
        return new LegacyBookingRollupEngine(
            new List<ILegacyBookingRollupModifier>
            {
                new LegacyBookingRollupCreatedAtOffsetAdjuster(1.15),
                new TitleLegacyBookingRollupAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.22, ["online"] = 0.73, ["retail"] = 1.2 }),
                new TieredLegacyBookingRollupAdjuster(
                    new List<double> { 1501.0, 5060.0, 6267.0, 9868.0 },
                    new List<double> { 0.88, 0.88, 1.16, 0.99 }),
                new FlaggedLegacyBookingRollupAdjuster(flags, "legacy-booking-rollup-tiered", 9.6),
                new MinimumLegacyBookingRollupAdjuster(40.3),
            },
            new List<string>(),
            0.6,
            17765.2);
    }

    public double Price(LegacyBookingRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyBookingRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
