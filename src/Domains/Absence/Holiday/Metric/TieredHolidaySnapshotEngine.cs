namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered holiday snapshot calculation.</summary>
public interface ITieredHolidaySnapshotAdjuster
{
    string Describe();
    double Adjust(TieredHolidaySnapshot item, double valueValue);
}

public sealed record PercentTieredHolidaySnapshotAdjuster(double Percent) : ITieredHolidaySnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredHolidaySnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredHolidaySnapshotAdjuster(double Cap) : ITieredHolidaySnapshotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredHolidaySnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredHolidaySnapshotScoreOffsetAdjuster(double Weight) : ITieredHolidaySnapshotAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(TieredHolidaySnapshot item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record TieredTieredHolidaySnapshotAdjuster(List<double> Bounds, List<double> Rates) : ITieredHolidaySnapshotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredHolidaySnapshot item, double valueValue)
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

public sealed record NameTieredHolidaySnapshotAdjuster(Dictionary<string, double> Factors) : ITieredHolidaySnapshotAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TieredHolidaySnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the tiered holiday snapshot steps in order and keeps a trace of each.</summary>
public sealed class TieredHolidaySnapshotEngine(List<ITieredHolidaySnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredHolidaySnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredHolidaySnapshotEngine DefaultTieredHolidaySnapshotEngine()
    {
        return new TieredHolidaySnapshotEngine(
            new List<ITieredHolidaySnapshotAdjuster>
            {
                new PercentTieredHolidaySnapshotAdjuster(15.7),
                new CapTieredHolidaySnapshotAdjuster(47927.0),
                new TieredHolidaySnapshotScoreOffsetAdjuster(1.35),
                new TieredTieredHolidaySnapshotAdjuster(
                    new List<double> { 5591.0, 8811.0 },
                    new List<double> { 1.0, 1.1 }),
                new NameTieredHolidaySnapshotAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.73, ["east"] = 1.07, ["west"] = 1.25 }),
            },
            new List<string>(),
            5.0,
            28074.4);
    }

    public double Evaluate(TieredHolidaySnapshot item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredHolidaySnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
