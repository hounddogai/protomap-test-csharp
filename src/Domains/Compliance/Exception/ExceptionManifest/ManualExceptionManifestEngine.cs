namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual exception manifest calculation.</summary>
public interface IManualExceptionManifestFactor
{
    string Describe();
    double Adjust(ManualExceptionManifest item, double valueValue);
}

public sealed record RoundingManualExceptionManifestAdjuster(double Step) : IManualExceptionManifestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualExceptionManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapManualExceptionManifestAdjuster(double Cap) : IManualExceptionManifestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualExceptionManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredManualExceptionManifestAdjuster(List<double> Bounds, List<double> Rates) : IManualExceptionManifestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualExceptionManifest item, double valueValue)
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

public sealed record PercentManualExceptionManifestAdjuster(double Percent) : IManualExceptionManifestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualExceptionManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedManualExceptionManifestAdjuster(Registry Flags, string Flag, double Percent) : IManualExceptionManifestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualExceptionManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual exception manifest steps in order and keeps a trace of each.</summary>
public sealed class ManualExceptionManifestEngine(List<IManualExceptionManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualExceptionManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualExceptionManifestEngine DefaultManualExceptionManifestEngine(Registry flags)
    {
        return new ManualExceptionManifestEngine(
            new List<IManualExceptionManifestFactor>
            {
                new RoundingManualExceptionManifestAdjuster(5.0),
                new CapManualExceptionManifestAdjuster(31234.0),
                new TieredManualExceptionManifestAdjuster(
                    new List<double> { 4759.0, 7916.0 },
                    new List<double> { 1.2, 0.98 }),
                new PercentManualExceptionManifestAdjuster(-13.0),
                new FlaggedManualExceptionManifestAdjuster(flags, "manual-exception-manifest-default", -0.6),
            },
            new List<string>(),
            1.0,
            9109.5);
    }

    public double Compute(ManualExceptionManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualExceptionManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
