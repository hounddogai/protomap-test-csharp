namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy retro pay snapshot calculation.</summary>
public interface ILegacyRetroPaySnapshotAdjuster
{
    string Describe();
    double Adjust(LegacyRetroPaySnapshot item, double valueValue);
}

public sealed record TieredLegacyRetroPaySnapshotAdjuster(List<double> Bounds, List<double> Rates) : ILegacyRetroPaySnapshotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyRetroPaySnapshot item, double valueValue)
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

public sealed record LegacyRetroPaySnapshotCreatedAtOffsetAdjuster(double Weight) : ILegacyRetroPaySnapshotAdjuster
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(LegacyRetroPaySnapshot item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record MinimumLegacyRetroPaySnapshotAdjuster(double Minimum) : ILegacyRetroPaySnapshotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyRetroPaySnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLegacyRetroPaySnapshotAdjuster(Registry Flags, string Flag, double Percent) : ILegacyRetroPaySnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyRetroPaySnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy retro pay snapshot steps in order and keeps a trace of each.</summary>
public sealed class LegacyRetroPaySnapshotEngine(List<ILegacyRetroPaySnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyRetroPaySnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyRetroPaySnapshotEngine DefaultLegacyRetroPaySnapshotEngine(Registry flags)
    {
        return new LegacyRetroPaySnapshotEngine(
            new List<ILegacyRetroPaySnapshotAdjuster>
            {
                new TieredLegacyRetroPaySnapshotAdjuster(
                    new List<double> { 4806.0, 4812.0 },
                    new List<double> { 1.02, 0.94 }),
                new LegacyRetroPaySnapshotCreatedAtOffsetAdjuster(1.85),
                new MinimumLegacyRetroPaySnapshotAdjuster(37.4),
                new FlaggedLegacyRetroPaySnapshotAdjuster(flags, "legacy-retro-pay-snapshot-pending", -2.3),
            },
            new List<string>(),
            2.6,
            45324.9);
    }

    public double Price(LegacyRetroPaySnapshot item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyRetroPaySnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
