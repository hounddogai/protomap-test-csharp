namespace Acme.Workforce.Compliance.Certification.Snapshot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tiered certification snapshot calculation.</summary>
public interface ITieredCertificationSnapshotAdjuster
{
    string Describe();
    double Adjust(TieredCertificationSnapshot item, double valueValue);
}

public sealed record CapTieredCertificationSnapshotAdjuster(double Cap) : ITieredCertificationSnapshotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredCertificationSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedTieredCertificationSnapshotAdjuster(Registry Flags, string Flag, double Percent) : ITieredCertificationSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredCertificationSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCertificationSnapshotSequenceOffsetAdjuster(double Weight) : ITieredCertificationSnapshotAdjuster
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(TieredCertificationSnapshot item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record PercentTieredCertificationSnapshotAdjuster(double Percent) : ITieredCertificationSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredCertificationSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceTieredCertificationSnapshotAdjuster(Dictionary<string, double> Factors) : ITieredCertificationSnapshotAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(TieredCertificationSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredTieredCertificationSnapshotAdjuster(List<double> Bounds, List<double> Rates) : ITieredCertificationSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredCertificationSnapshot item, double valueValue)
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

/// <summary>Runs the tiered certification snapshot steps in order and keeps a trace of each.</summary>
public sealed class TieredCertificationSnapshotEngine(List<ITieredCertificationSnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredCertificationSnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredCertificationSnapshotEngine DefaultTieredCertificationSnapshotEngine(Registry flags)
    {
        return new TieredCertificationSnapshotEngine(
            new List<ITieredCertificationSnapshotAdjuster>
            {
                new CapTieredCertificationSnapshotAdjuster(13325.0),
                new FlaggedTieredCertificationSnapshotAdjuster(flags, "tiered-certification-snapshot-manual", -6.5),
                new TieredCertificationSnapshotSequenceOffsetAdjuster(1.28),
                new PercentTieredCertificationSnapshotAdjuster(9.9),
                new SourceTieredCertificationSnapshotAdjuster(new Dictionary<string, double> { ["central"] = 0.99 }),
                new TieredTieredCertificationSnapshotAdjuster(
                    new List<double> { 5734.0, 7420.0, 9118.0, 9705.0 },
                    new List<double> { 0.85, 1.16, 0.86, 1.03 }),
            },
            new List<string>(),
            3.1,
            3886.2);
    }

    public double Compute(TieredCertificationSnapshot item)
    {
        var current = item.WeightedExposure();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<TieredCertificationSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
