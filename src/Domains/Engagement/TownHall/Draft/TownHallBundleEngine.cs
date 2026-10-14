namespace Acme.Workforce.Engagement.TownHall.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall bundle calculation.</summary>
public interface ITownHallBundleFactor
{
    string Describe();
    double Adjust(TownHallBundle item, double valueValue);
}

public sealed record CapTownHallBundleAdjuster(double Cap) : ITownHallBundleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTownHallBundleAdjuster(List<double> Bounds, List<double> Rates) : ITownHallBundleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallBundle item, double valueValue)
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

public sealed record TownHallBundleDiscountOffsetAdjuster(double Weight) : ITownHallBundleFactor
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(TownHallBundle item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record RoundingTownHallBundleAdjuster(double Step) : ITownHallBundleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SegmentTownHallBundleAdjuster(Dictionary<string, double> Factors) : ITownHallBundleFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(TownHallBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedTownHallBundleAdjuster(Registry Flags, string Flag, double Percent) : ITownHallBundleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the town hall bundle steps in order and keeps a trace of each.</summary>
public sealed class TownHallBundleEngine(List<ITownHallBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallBundleEngine DefaultTownHallBundleEngine(Registry flags)
    {
        return new TownHallBundleEngine(
            new List<ITownHallBundleFactor>
            {
                new CapTownHallBundleAdjuster(5813.0),
                new TieredTownHallBundleAdjuster(new List<double> { 6548.0, 9085.0 }, new List<double> { 1.1, 1.14 }),
                new TownHallBundleDiscountOffsetAdjuster(1.52),
                new RoundingTownHallBundleAdjuster(1.0),
                new SegmentTownHallBundleAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.92, ["retail"] = 1.07, ["west"] = 0.98 }),
                new FlaggedTownHallBundleAdjuster(flags, "town-hall-bundle-global", -3.6),
            },
            new List<string>(),
            0.1,
            46795.5);
    }

    public double Run(TownHallBundle item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
