namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback position quota calculation.</summary>
public interface IFallbackPositionQuotaFactor
{
    string Describe();
    double Adjust(FallbackPositionQuota item, double valueValue);
}

public sealed record RegionFallbackPositionQuotaAdjuster(Dictionary<string, double> Factors) : IFallbackPositionQuotaFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(FallbackPositionQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingFallbackPositionQuotaAdjuster(double Step) : IFallbackPositionQuotaFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackPositionQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedFallbackPositionQuotaAdjuster(Registry Flags, string Flag, double Percent) : IFallbackPositionQuotaFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackPositionQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFallbackPositionQuotaAdjuster(double Cap) : IFallbackPositionQuotaFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackPositionQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumFallbackPositionQuotaAdjuster(double Minimum) : IFallbackPositionQuotaFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackPositionQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredFallbackPositionQuotaAdjuster(List<double> Bounds, List<double> Rates) : IFallbackPositionQuotaFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackPositionQuota item, double valueValue)
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

/// <summary>Runs the fallback position quota steps in order and keeps a trace of each.</summary>
public sealed class FallbackPositionQuotaEngine(List<IFallbackPositionQuotaFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPositionQuotaFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPositionQuotaEngine DefaultFallbackPositionQuotaEngine(Registry flags)
    {
        return new FallbackPositionQuotaEngine(
            new List<IFallbackPositionQuotaFactor>
            {
                new RegionFallbackPositionQuotaAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.32, ["west"] = 1.36 }),
                new RoundingFallbackPositionQuotaAdjuster(0.1),
                new FlaggedFallbackPositionQuotaAdjuster(flags, "fallback-position-quota-rolling", 1.4),
                new CapFallbackPositionQuotaAdjuster(1348.0),
                new MinimumFallbackPositionQuotaAdjuster(48.8),
                new TieredFallbackPositionQuotaAdjuster(
                    new List<double> { 466.0, 3227.0 },
                    new List<double> { 0.92, 0.96 }),
            },
            new List<string>(),
            4.1,
            7702.9);
    }

    public double Run(FallbackPositionQuota item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPositionQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
