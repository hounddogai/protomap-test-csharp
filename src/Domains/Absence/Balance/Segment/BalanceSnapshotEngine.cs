namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance snapshot calculation.</summary>
public interface IBalanceSnapshotFactor
{
    string Describe();
    double Adjust(BalanceSnapshot item, double valueValue);
}

public sealed record RoundingBalanceSnapshotAdjuster(double Step) : IBalanceSnapshotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BalanceSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredBalanceSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IBalanceSnapshotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BalanceSnapshot item, double valueValue)
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

public sealed record CapBalanceSnapshotAdjuster(double Cap) : IBalanceSnapshotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BalanceSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record BalanceSnapshotVersionOffsetAdjuster(double Weight) : IBalanceSnapshotFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(BalanceSnapshot item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record MinimumBalanceSnapshotAdjuster(double Minimum) : IBalanceSnapshotFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalanceSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SegmentBalanceSnapshotAdjuster(Dictionary<string, double> Factors) : IBalanceSnapshotFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(BalanceSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the balance snapshot steps in order and keeps a trace of each.</summary>
public sealed class BalanceSnapshotEngine(List<IBalanceSnapshotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceSnapshotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceSnapshotEngine DefaultBalanceSnapshotEngine()
    {
        return new BalanceSnapshotEngine(
            new List<IBalanceSnapshotFactor>
            {
                new RoundingBalanceSnapshotAdjuster(0.25),
                new TieredBalanceSnapshotAdjuster(
                    new List<double> { 3026.0, 3587.0, 8149.0 },
                    new List<double> { 0.84, 1.17, 1.0 }),
                new CapBalanceSnapshotAdjuster(40778.0),
                new BalanceSnapshotVersionOffsetAdjuster(1.02),
                new MinimumBalanceSnapshotAdjuster(5.8),
                new SegmentBalanceSnapshotAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.29, ["east"] = 1.33, ["retail"] = 1.03 }),
            },
            new List<string>(),
            0.1,
            55383.4);
    }

    public double Compute(BalanceSnapshot item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
