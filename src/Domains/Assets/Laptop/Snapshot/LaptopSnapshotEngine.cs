namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop snapshot calculation.</summary>
public interface ILaptopSnapshotAdjuster
{
    string Describe();
    double Adjust(LaptopSnapshot item, double valueValue);
}

public sealed record LaptopSnapshotUpdatedAtOffsetAdjuster(double Weight) : ILaptopSnapshotAdjuster
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(LaptopSnapshot item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record RoundingLaptopSnapshotAdjuster(double Step) : ILaptopSnapshotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLaptopSnapshotAdjuster(List<double> Bounds, List<double> Rates) : ILaptopSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LaptopSnapshot item, double valueValue)
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

public sealed record CapLaptopSnapshotAdjuster(double Cap) : ILaptopSnapshotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the laptop snapshot steps in order and keeps a trace of each.</summary>
public sealed class LaptopSnapshotEngine(List<ILaptopSnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopSnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopSnapshotEngine DefaultLaptopSnapshotEngine()
    {
        return new LaptopSnapshotEngine(
            new List<ILaptopSnapshotAdjuster>
            {
                new LaptopSnapshotUpdatedAtOffsetAdjuster(0.39),
                new RoundingLaptopSnapshotAdjuster(5.0),
                new TieredLaptopSnapshotAdjuster(
                    new List<double> { 4938.0, 5410.0, 7190.0, 7261.0 },
                    new List<double> { 0.97, 1.1, 1.0, 1.01 }),
                new CapLaptopSnapshotAdjuster(46651.0),
            },
            new List<string>(),
            1.3,
            28237.2);
    }

    public double Evaluate(LaptopSnapshot item)
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

    public Dictionary<string, double> EvaluateAll(List<LaptopSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
