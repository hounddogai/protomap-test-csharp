namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal reorg snapshot calculation.</summary>
public interface ISeasonalReorgSnapshotModifier
{
    string Describe();
    double Adjust(SeasonalReorgSnapshot item, double valueValue);
}

public sealed record TieredSeasonalReorgSnapshotAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalReorgSnapshotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalReorgSnapshot item, double valueValue)
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

public sealed record CapSeasonalReorgSnapshotAdjuster(double Cap) : ISeasonalReorgSnapshotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalReorgSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSeasonalReorgSnapshotAdjuster(double Step) : ISeasonalReorgSnapshotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalReorgSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSeasonalReorgSnapshotAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalReorgSnapshotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalReorgSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal reorg snapshot steps in order and keeps a trace of each.</summary>
public sealed class SeasonalReorgSnapshotEngine(List<ISeasonalReorgSnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalReorgSnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalReorgSnapshotEngine DefaultSeasonalReorgSnapshotEngine(Registry flags)
    {
        return new SeasonalReorgSnapshotEngine(
            new List<ISeasonalReorgSnapshotModifier>
            {
                new TieredSeasonalReorgSnapshotAdjuster(
                    new List<double> { 7152.0, 8156.0 },
                    new List<double> { 1.0, 1.15 }),
                new CapSeasonalReorgSnapshotAdjuster(41222.0),
                new RoundingSeasonalReorgSnapshotAdjuster(0.5),
                new FlaggedSeasonalReorgSnapshotAdjuster(flags, "seasonal-reorg-snapshot-capped", -4.1),
            },
            new List<string>(),
            3.9,
            85766.5);
    }

    public double Price(SeasonalReorgSnapshot item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalReorgSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
