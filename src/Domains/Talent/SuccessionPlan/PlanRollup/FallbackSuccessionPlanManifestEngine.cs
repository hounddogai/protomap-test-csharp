namespace Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback succession plan manifest calculation.</summary>
public interface IFallbackSuccessionPlanManifestFactor
{
    string Describe();
    double Adjust(FallbackSuccessionPlanManifest item, double valueValue);
}

public sealed record CapFallbackSuccessionPlanManifestAdjuster(double Cap) : IFallbackSuccessionPlanManifestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackSuccessionPlanManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FallbackSuccessionPlanManifestRatingOffsetAdjuster(double Weight) : IFallbackSuccessionPlanManifestFactor
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(FallbackSuccessionPlanManifest item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record NameFallbackSuccessionPlanManifestAdjuster(Dictionary<string, double> Factors) : IFallbackSuccessionPlanManifestFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(FallbackSuccessionPlanManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedFallbackSuccessionPlanManifestAdjuster(Registry Flags, string Flag, double Percent) : IFallbackSuccessionPlanManifestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackSuccessionPlanManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFallbackSuccessionPlanManifestAdjuster(double Step) : IFallbackSuccessionPlanManifestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackSuccessionPlanManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback succession plan manifest steps in order and keeps a trace of each.</summary>
public sealed class FallbackSuccessionPlanManifestEngine(List<IFallbackSuccessionPlanManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackSuccessionPlanManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackSuccessionPlanManifestEngine DefaultFallbackSuccessionPlanManifestEngine(Registry flags)
    {
        return new FallbackSuccessionPlanManifestEngine(
            new List<IFallbackSuccessionPlanManifestFactor>
            {
                new CapFallbackSuccessionPlanManifestAdjuster(21162.0),
                new FallbackSuccessionPlanManifestRatingOffsetAdjuster(1.12),
                new NameFallbackSuccessionPlanManifestAdjuster(new Dictionary<string, double> { ["north"] = 0.9 }),
                new FlaggedFallbackSuccessionPlanManifestAdjuster(
                    flags,
                    "fallback-succession-plan-manifest-pending",
                    3.5),
                new RoundingFallbackSuccessionPlanManifestAdjuster(1.0),
            },
            new List<string>(),
            1.0,
            5357.4);
    }

    public double Price(FallbackSuccessionPlanManifest item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackSuccessionPlanManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
