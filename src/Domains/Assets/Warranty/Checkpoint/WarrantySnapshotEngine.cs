namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty snapshot calculation.</summary>
public interface IWarrantySnapshotAdjuster
{
    string Describe();
    double Adjust(WarrantySnapshot item, double valueValue);
}

public sealed record PercentWarrantySnapshotAdjuster(double Percent) : IWarrantySnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WarrantySnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWarrantySnapshotAdjuster(double Cap) : IWarrantySnapshotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantySnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedWarrantySnapshotAdjuster(Registry Flags, string Flag, double Percent) : IWarrantySnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantySnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record WarrantySnapshotBudgetOffsetAdjuster(double Weight) : IWarrantySnapshotAdjuster
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(WarrantySnapshot item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

/// <summary>Runs the warranty snapshot steps in order and keeps a trace of each.</summary>
public sealed class WarrantySnapshotEngine(List<IWarrantySnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantySnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantySnapshotEngine DefaultWarrantySnapshotEngine(Registry flags)
    {
        return new WarrantySnapshotEngine(
            new List<IWarrantySnapshotAdjuster>
            {
                new PercentWarrantySnapshotAdjuster(22.0),
                new CapWarrantySnapshotAdjuster(40250.0),
                new FlaggedWarrantySnapshotAdjuster(flags, "warranty-snapshot-scheduled", 4.0),
                new WarrantySnapshotBudgetOffsetAdjuster(0.79),
            },
            new List<string>(),
            4.1,
            75299.6);
    }

    public double Price(WarrantySnapshot item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantySnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
