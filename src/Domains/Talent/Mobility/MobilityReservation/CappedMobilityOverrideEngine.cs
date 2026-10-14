namespace Acme.Workforce.Talent.Mobility.MobilityReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped mobility override calculation.</summary>
public interface ICappedMobilityOverrideAdjuster
{
    string Describe();
    double Adjust(CappedMobilityOverride item, double valueValue);
}

public sealed record PercentCappedMobilityOverrideAdjuster(double Percent) : ICappedMobilityOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedMobilityOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedMobilityOverrideAdjuster(double Step) : ICappedMobilityOverrideAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedMobilityOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedCappedMobilityOverrideAdjuster(Registry Flags, string Flag, double Percent) : ICappedMobilityOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedMobilityOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedMobilityOverrideAdjuster(List<double> Bounds, List<double> Rates) : ICappedMobilityOverrideAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedMobilityOverride item, double valueValue)
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

/// <summary>Runs the capped mobility override steps in order and keeps a trace of each.</summary>
public sealed class CappedMobilityOverrideEngine(List<ICappedMobilityOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedMobilityOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedMobilityOverrideEngine DefaultCappedMobilityOverrideEngine(Registry flags)
    {
        return new CappedMobilityOverrideEngine(
            new List<ICappedMobilityOverrideAdjuster>
            {
                new PercentCappedMobilityOverrideAdjuster(-13.4),
                new RoundingCappedMobilityOverrideAdjuster(5.0),
                new FlaggedCappedMobilityOverrideAdjuster(flags, "capped-mobility-override-fallback", 7.0),
                new TieredCappedMobilityOverrideAdjuster(
                    new List<double> { 4299.0, 7967.0 },
                    new List<double> { 0.83, 1.17 }),
            },
            new List<string>(),
            4.2,
            3714.5);
    }

    public double Price(CappedMobilityOverride item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedMobilityOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
