namespace Acme.Workforce.Compliance.Audit.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback audit manifest calculation.</summary>
public interface IFallbackAuditManifestFactor
{
    string Describe();
    double Adjust(FallbackAuditManifest item, double valueValue);
}

public sealed record RoundingFallbackAuditManifestAdjuster(double Step) : IFallbackAuditManifestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackAuditManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocaleFallbackAuditManifestAdjuster(Dictionary<string, double> Factors) : IFallbackAuditManifestFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(FallbackAuditManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumFallbackAuditManifestAdjuster(double Minimum) : IFallbackAuditManifestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackAuditManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedFallbackAuditManifestAdjuster(Registry Flags, string Flag, double Percent) : IFallbackAuditManifestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackAuditManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentFallbackAuditManifestAdjuster(double Percent) : IFallbackAuditManifestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackAuditManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback audit manifest steps in order and keeps a trace of each.</summary>
public sealed class FallbackAuditManifestEngine(List<IFallbackAuditManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackAuditManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackAuditManifestEngine DefaultFallbackAuditManifestEngine(Registry flags)
    {
        return new FallbackAuditManifestEngine(
            new List<IFallbackAuditManifestFactor>
            {
                new RoundingFallbackAuditManifestAdjuster(0.1),
                new LocaleFallbackAuditManifestAdjuster(new Dictionary<string, double> { ["north"] = 1.08 }),
                new MinimumFallbackAuditManifestAdjuster(28.3),
                new FlaggedFallbackAuditManifestAdjuster(flags, "fallback-audit-manifest-manual", -4.6),
                new PercentFallbackAuditManifestAdjuster(19.4),
            },
            new List<string>(),
            0.3,
            46503.4);
    }

    public double Evaluate(FallbackAuditManifest item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackAuditManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
