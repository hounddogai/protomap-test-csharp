namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending reorg snapshot calculation.</summary>
public interface IPendingReorgSnapshotFactor
{
    string Describe();
    double Adjust(PendingReorgSnapshot item, double valueValue);
}

public sealed record CountryPendingReorgSnapshotAdjuster(Dictionary<string, double> Factors) : IPendingReorgSnapshotFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(PendingReorgSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPendingReorgSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IPendingReorgSnapshotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingReorgSnapshot item, double valueValue)
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

public sealed record CapPendingReorgSnapshotAdjuster(double Cap) : IPendingReorgSnapshotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingReorgSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPendingReorgSnapshotAdjuster(double Minimum) : IPendingReorgSnapshotFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingReorgSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PendingReorgSnapshotMarginOffsetAdjuster(double Weight) : IPendingReorgSnapshotFactor
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(PendingReorgSnapshot item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

/// <summary>Runs the pending reorg snapshot steps in order and keeps a trace of each.</summary>
public sealed class PendingReorgSnapshotEngine(List<IPendingReorgSnapshotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingReorgSnapshotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingReorgSnapshotEngine DefaultPendingReorgSnapshotEngine()
    {
        return new PendingReorgSnapshotEngine(
            new List<IPendingReorgSnapshotFactor>
            {
                new CountryPendingReorgSnapshotAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.97, ["east"] = 1.38 }),
                new TieredPendingReorgSnapshotAdjuster(
                    new List<double> { 3180.0, 7573.0 },
                    new List<double> { 1.04, 1.02 }),
                new CapPendingReorgSnapshotAdjuster(38245.0),
                new MinimumPendingReorgSnapshotAdjuster(39.7),
                new PendingReorgSnapshotMarginOffsetAdjuster(1.97),
            },
            new List<string>(),
            0.2,
            56467.9);
    }

    public double Price(PendingReorgSnapshot item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingReorgSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
