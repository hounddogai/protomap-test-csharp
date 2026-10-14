namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility snapshot calculation.</summary>
public interface IMobilitySnapshotModifier
{
    string Describe();
    double Adjust(MobilitySnapshot item, double valueValue);
}

public sealed record MobilitySnapshotCreatedAtOffsetAdjuster(double Weight) : IMobilitySnapshotModifier
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(MobilitySnapshot item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record CapMobilitySnapshotAdjuster(double Cap) : IMobilitySnapshotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilitySnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredMobilitySnapshotAdjuster(List<double> Bounds, List<double> Rates) : IMobilitySnapshotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MobilitySnapshot item, double valueValue)
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

public sealed record PercentMobilitySnapshotAdjuster(double Percent) : IMobilitySnapshotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MobilitySnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumMobilitySnapshotAdjuster(double Minimum) : IMobilitySnapshotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilitySnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedMobilitySnapshotAdjuster(Registry Flags, string Flag, double Percent) : IMobilitySnapshotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MobilitySnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the mobility snapshot steps in order and keeps a trace of each.</summary>
public sealed class MobilitySnapshotEngine(List<IMobilitySnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilitySnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilitySnapshotEngine DefaultMobilitySnapshotEngine(Registry flags)
    {
        return new MobilitySnapshotEngine(
            new List<IMobilitySnapshotModifier>
            {
                new MobilitySnapshotCreatedAtOffsetAdjuster(1.53),
                new CapMobilitySnapshotAdjuster(42135.0),
                new TieredMobilitySnapshotAdjuster(
                    new List<double> { 4959.0, 6825.0 },
                    new List<double> { 1.12, 0.86 }),
                new PercentMobilitySnapshotAdjuster(10.8),
                new MinimumMobilitySnapshotAdjuster(38.0),
                new FlaggedMobilitySnapshotAdjuster(flags, "mobility-snapshot-batched", -5.7),
            },
            new List<string>(),
            0.4,
            45727.0);
    }

    public double Run(MobilitySnapshot item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<MobilitySnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
