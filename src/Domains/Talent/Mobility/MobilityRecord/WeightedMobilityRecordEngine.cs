namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted mobility record calculation.</summary>
public interface IWeightedMobilityRecordStep
{
    string Describe();
    double Adjust(WeightedMobilityRecord item, double valueValue);
}

public sealed record RoundingWeightedMobilityRecordAdjuster(double Step) : IWeightedMobilityRecordStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedMobilityRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedWeightedMobilityRecordAdjuster(Registry Flags, string Flag, double Percent) : IWeightedMobilityRecordStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedMobilityRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWeightedMobilityRecordAdjuster(List<double> Bounds, List<double> Rates) : IWeightedMobilityRecordStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedMobilityRecord item, double valueValue)
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

/// <summary>Runs the weighted mobility record steps in order and keeps a trace of each.</summary>
public sealed class WeightedMobilityRecordEngine(List<IWeightedMobilityRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedMobilityRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedMobilityRecordEngine DefaultWeightedMobilityRecordEngine(Registry flags)
    {
        return new WeightedMobilityRecordEngine(
            new List<IWeightedMobilityRecordStep>
            {
                new RoundingWeightedMobilityRecordAdjuster(0.25),
                new FlaggedWeightedMobilityRecordAdjuster(flags, "weighted-mobility-record-global", -5.5),
                new TieredWeightedMobilityRecordAdjuster(
                    new List<double> { 4554.0, 8019.0 },
                    new List<double> { 1.08, 1.12 }),
            },
            new List<string>(),
            2.8,
            3511.4);
    }

    public double Compute(WeightedMobilityRecord item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedMobilityRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
