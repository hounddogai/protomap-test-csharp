namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered warranty override calculation.</summary>
public interface ITieredWarrantyOverrideFactor
{
    string Describe();
    double Adjust(TieredWarrantyOverride item, double valueValue);
}

public sealed record FlaggedTieredWarrantyOverrideAdjuster(Registry Flags, string Flag, double Percent) : ITieredWarrantyOverrideFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredWarrantyOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWarrantyOverrideVersionOffsetAdjuster(double Weight) : ITieredWarrantyOverrideFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(TieredWarrantyOverride item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record CapTieredWarrantyOverrideAdjuster(double Cap) : ITieredWarrantyOverrideFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredWarrantyOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTieredWarrantyOverrideAdjuster(double Percent) : ITieredWarrantyOverrideFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredWarrantyOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredWarrantyOverrideAdjuster(double Step) : ITieredWarrantyOverrideFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredWarrantyOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTieredWarrantyOverrideAdjuster(List<double> Bounds, List<double> Rates) : ITieredWarrantyOverrideFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredWarrantyOverride item, double valueValue)
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

/// <summary>Runs the tiered warranty override steps in order and keeps a trace of each.</summary>
public sealed class TieredWarrantyOverrideEngine(List<ITieredWarrantyOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredWarrantyOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredWarrantyOverrideEngine DefaultTieredWarrantyOverrideEngine(Registry flags)
    {
        return new TieredWarrantyOverrideEngine(
            new List<ITieredWarrantyOverrideFactor>
            {
                new FlaggedTieredWarrantyOverrideAdjuster(flags, "tiered-warranty-override-weighted", -9.3),
                new TieredWarrantyOverrideVersionOffsetAdjuster(1.52),
                new CapTieredWarrantyOverrideAdjuster(49977.0),
                new PercentTieredWarrantyOverrideAdjuster(-2.0),
                new RoundingTieredWarrantyOverrideAdjuster(0.5),
                new TieredTieredWarrantyOverrideAdjuster(
                    new List<double> { 3849.0, 7369.0, 7555.0, 8801.0 },
                    new List<double> { 1.13, 0.87, 1.07, 0.94 }),
            },
            new List<string>(),
            0.2,
            85258.4);
    }

    public double Compute(TieredWarrantyOverride item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredWarrantyOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
