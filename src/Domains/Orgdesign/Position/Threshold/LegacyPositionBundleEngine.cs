namespace Acme.Workforce.Orgdesign.Position.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy position bundle calculation.</summary>
public interface ILegacyPositionBundleAdjuster
{
    string Describe();
    double Adjust(LegacyPositionBundle item, double valueValue);
}

public sealed record RoundingLegacyPositionBundleAdjuster(double Step) : ILegacyPositionBundleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyPositionBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLegacyPositionBundleAdjuster(List<double> Bounds, List<double> Rates) : ILegacyPositionBundleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyPositionBundle item, double valueValue)
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

public sealed record LegacyPositionBundlePriorityOffsetAdjuster(double Weight) : ILegacyPositionBundleAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(LegacyPositionBundle item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record FlaggedLegacyPositionBundleAdjuster(Registry Flags, string Flag, double Percent) : ILegacyPositionBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyPositionBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy position bundle steps in order and keeps a trace of each.</summary>
public sealed class LegacyPositionBundleEngine(List<ILegacyPositionBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPositionBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPositionBundleEngine DefaultLegacyPositionBundleEngine(Registry flags)
    {
        return new LegacyPositionBundleEngine(
            new List<ILegacyPositionBundleAdjuster>
            {
                new RoundingLegacyPositionBundleAdjuster(0.05),
                new TieredLegacyPositionBundleAdjuster(
                    new List<double> { 1009.0, 1561.0, 4359.0, 6174.0 },
                    new List<double> { 1.01, 1.16, 0.9, 1.06 }),
                new LegacyPositionBundlePriorityOffsetAdjuster(0.89),
                new FlaggedLegacyPositionBundleAdjuster(flags, "legacy-position-bundle-batched", 9.5),
            },
            new List<string>(),
            3.7,
            88531.8);
    }

    public double Compute(LegacyPositionBundle item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPositionBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
