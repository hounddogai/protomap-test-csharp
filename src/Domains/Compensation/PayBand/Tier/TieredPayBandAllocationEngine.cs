namespace Acme.Workforce.Compensation.PayBand.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered pay band allocation calculation.</summary>
public interface ITieredPayBandAllocationFactor
{
    string Describe();
    double Adjust(TieredPayBandAllocation item, double valueValue);
}

public sealed record SegmentTieredPayBandAllocationAdjuster(Dictionary<string, double> Factors) : ITieredPayBandAllocationFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(TieredPayBandAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedTieredPayBandAllocationAdjuster(Registry Flags, string Flag, double Percent) : ITieredPayBandAllocationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredPayBandAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredPayBandAllocationAdjuster(double Cap) : ITieredPayBandAllocationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredPayBandAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPayBandAllocationRevisionOffsetAdjuster(double Weight) : ITieredPayBandAllocationFactor
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(TieredPayBandAllocation item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

/// <summary>Runs the tiered pay band allocation steps in order and keeps a trace of each.</summary>
public sealed class TieredPayBandAllocationEngine(List<ITieredPayBandAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredPayBandAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredPayBandAllocationEngine DefaultTieredPayBandAllocationEngine(Registry flags)
    {
        return new TieredPayBandAllocationEngine(
            new List<ITieredPayBandAllocationFactor>
            {
                new SegmentTieredPayBandAllocationAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.11, ["central"] = 0.89 }),
                new FlaggedTieredPayBandAllocationAdjuster(flags, "tiered-pay-band-allocation-capped", 1.8),
                new CapTieredPayBandAllocationAdjuster(2528.0),
                new TieredPayBandAllocationRevisionOffsetAdjuster(0.16),
            },
            new List<string>(),
            4.1,
            58945.1);
    }

    public double Run(TieredPayBandAllocation item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredPayBandAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
