namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate snapshot calculation.</summary>
public interface IRateSnapshotFactor
{
    string Describe();
    double Adjust(RateSnapshot item, double valueValue);
}

public sealed record PercentRateSnapshotAdjuster(double Percent) : IRateSnapshotFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRateSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IRateSnapshotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateSnapshot item, double valueValue)
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

public sealed record CapRateSnapshotAdjuster(double Cap) : IRateSnapshotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RateSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rate snapshot steps in order and keeps a trace of each.</summary>
public sealed class RateSnapshotEngine(List<IRateSnapshotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateSnapshotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateSnapshotEngine DefaultRateSnapshotEngine()
    {
        return new RateSnapshotEngine(
            new List<IRateSnapshotFactor>
            {
                new PercentRateSnapshotAdjuster(19.9),
                new TieredRateSnapshotAdjuster(
                    new List<double> { 1823.0, 3129.0, 3953.0, 4187.0 },
                    new List<double> { 1.07, 1.15, 0.91, 0.85 }),
                new CapRateSnapshotAdjuster(3525.0),
            },
            new List<string>(),
            0.7,
            35390.9);
    }

    public double Run(RateSnapshot item)
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

    public Dictionary<string, double> EvaluateAll(List<RateSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
