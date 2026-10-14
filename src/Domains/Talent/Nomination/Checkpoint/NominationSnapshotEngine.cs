namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the nomination snapshot calculation.</summary>
public interface INominationSnapshotFactor
{
    string Describe();
    double Adjust(NominationSnapshot item, double valueValue);
}

public sealed record FlaggedNominationSnapshotAdjuster(Registry Flags, string Flag, double Percent) : INominationSnapshotFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredNominationSnapshotAdjuster(List<double> Bounds, List<double> Rates) : INominationSnapshotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationSnapshot item, double valueValue)
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

public sealed record NominationSnapshotRatingOffsetAdjuster(double Weight) : INominationSnapshotFactor
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(NominationSnapshot item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record NameNominationSnapshotAdjuster(Dictionary<string, double> Factors) : INominationSnapshotFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(NominationSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the nomination snapshot steps in order and keeps a trace of each.</summary>
public sealed class NominationSnapshotEngine(List<INominationSnapshotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationSnapshotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationSnapshotEngine DefaultNominationSnapshotEngine(Registry flags)
    {
        return new NominationSnapshotEngine(
            new List<INominationSnapshotFactor>
            {
                new FlaggedNominationSnapshotAdjuster(flags, "nomination-snapshot-fallback", -10.0),
                new TieredNominationSnapshotAdjuster(
                    new List<double> { 2698.0, 3415.0, 4603.0 },
                    new List<double> { 1.07, 1.01, 0.9 }),
                new NominationSnapshotRatingOffsetAdjuster(1.13),
                new NameNominationSnapshotAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.26, ["online"] = 1.04, ["south"] = 1.06 }),
            },
            new List<string>(),
            4.6,
            70468.3);
    }

    public double Evaluate(NominationSnapshot item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<NominationSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
