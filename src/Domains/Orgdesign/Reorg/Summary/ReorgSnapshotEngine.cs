namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg snapshot calculation.</summary>
public interface IReorgSnapshotModifier
{
    string Describe();
    double Adjust(ReorgSnapshot item, double valueValue);
}

public sealed record RoundingReorgSnapshotAdjuster(double Step) : IReorgSnapshotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredReorgSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IReorgSnapshotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgSnapshot item, double valueValue)
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

public sealed record RegionReorgSnapshotAdjuster(Dictionary<string, double> Factors) : IReorgSnapshotModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(ReorgSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentReorgSnapshotAdjuster(double Percent) : IReorgSnapshotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReorgSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the reorg snapshot steps in order and keeps a trace of each.</summary>
public sealed class ReorgSnapshotEngine(List<IReorgSnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgSnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgSnapshotEngine DefaultReorgSnapshotEngine()
    {
        return new ReorgSnapshotEngine(
            new List<IReorgSnapshotModifier>
            {
                new RoundingReorgSnapshotAdjuster(0.1),
                new TieredReorgSnapshotAdjuster(
                    new List<double> { 546.0, 4264.0, 6695.0, 7168.0 },
                    new List<double> { 1.07, 0.84, 1.03, 0.84 }),
                new RegionReorgSnapshotAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.04, ["north"] = 0.87, ["online"] = 0.72 }),
                new PercentReorgSnapshotAdjuster(-7.0),
            },
            new List<string>(),
            2.9,
            28920.4);
    }

    public double Run(ReorgSnapshot item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
