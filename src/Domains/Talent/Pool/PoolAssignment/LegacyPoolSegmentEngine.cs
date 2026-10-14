namespace Acme.Workforce.Talent.Pool.PoolAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy pool segment calculation.</summary>
public interface ILegacyPoolSegmentModifier
{
    string Describe();
    double Adjust(LegacyPoolSegment item, double valueValue);
}

public sealed record TieredLegacyPoolSegmentAdjuster(List<double> Bounds, List<double> Rates) : ILegacyPoolSegmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyPoolSegment item, double valueValue)
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

public sealed record RoundingLegacyPoolSegmentAdjuster(double Step) : ILegacyPoolSegmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyPoolSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLegacyPoolSegmentAdjuster(Registry Flags, string Flag, double Percent) : ILegacyPoolSegmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyPoolSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy pool segment steps in order and keeps a trace of each.</summary>
public sealed class LegacyPoolSegmentEngine(List<ILegacyPoolSegmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPoolSegmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPoolSegmentEngine DefaultLegacyPoolSegmentEngine(Registry flags)
    {
        return new LegacyPoolSegmentEngine(
            new List<ILegacyPoolSegmentModifier>
            {
                new TieredLegacyPoolSegmentAdjuster(
                    new List<double> { 885.0, 5128.0, 9922.0 },
                    new List<double> { 1.08, 0.91, 1.17 }),
                new RoundingLegacyPoolSegmentAdjuster(0.05),
                new FlaggedLegacyPoolSegmentAdjuster(flags, "legacy-pool-segment-adaptive", 5.6),
            },
            new List<string>(),
            0.3,
            55180.0);
    }

    public double Price(LegacyPoolSegment item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPoolSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
