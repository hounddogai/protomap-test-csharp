namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default attestation rollup calculation.</summary>
public interface IDefaultAttestationRollupAdjuster
{
    string Describe();
    double Adjust(DefaultAttestationRollup item, double valueValue);
}

public sealed record CapDefaultAttestationRollupAdjuster(double Cap) : IDefaultAttestationRollupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultAttestationRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDefaultAttestationRollupAdjuster(Registry Flags, string Flag, double Percent) : IDefaultAttestationRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultAttestationRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultAttestationRollupAdjuster(List<double> Bounds, List<double> Rates) : IDefaultAttestationRollupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultAttestationRollup item, double valueValue)
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

public sealed record MinimumDefaultAttestationRollupAdjuster(double Minimum) : IDefaultAttestationRollupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultAttestationRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDefaultAttestationRollupAdjuster(double Percent) : IDefaultAttestationRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultAttestationRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default attestation rollup steps in order and keeps a trace of each.</summary>
public sealed class DefaultAttestationRollupEngine(List<IDefaultAttestationRollupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultAttestationRollupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultAttestationRollupEngine DefaultDefaultAttestationRollupEngine(Registry flags)
    {
        return new DefaultAttestationRollupEngine(
            new List<IDefaultAttestationRollupAdjuster>
            {
                new CapDefaultAttestationRollupAdjuster(24710.0),
                new FlaggedDefaultAttestationRollupAdjuster(flags, "default-attestation-rollup-lenient", 9.8),
                new TieredDefaultAttestationRollupAdjuster(
                    new List<double> { 3805.0, 6135.0, 9059.0 },
                    new List<double> { 0.96, 1.08, 1.16 }),
                new MinimumDefaultAttestationRollupAdjuster(14.6),
                new PercentDefaultAttestationRollupAdjuster(14.5),
            },
            new List<string>(),
            3.9,
            72233.7);
    }

    public double Run(DefaultAttestationRollup item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultAttestationRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
