namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop record calculation.</summary>
public interface ILaptopRecordAdjuster
{
    string Describe();
    double Adjust(LaptopRecord item, double valueValue);
}

public sealed record PercentLaptopRecordAdjuster(double Percent) : ILaptopRecordAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLaptopRecordAdjuster(Registry Flags, string Flag, double Percent) : ILaptopRecordAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LaptopRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionLaptopRecordAdjuster(Dictionary<string, double> Factors) : ILaptopRecordAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(LaptopRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLaptopRecordAdjuster(List<double> Bounds, List<double> Rates) : ILaptopRecordAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LaptopRecord item, double valueValue)
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

/// <summary>Runs the laptop record steps in order and keeps a trace of each.</summary>
public sealed class LaptopRecordEngine(List<ILaptopRecordAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopRecordAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopRecordEngine DefaultLaptopRecordEngine(Registry flags)
    {
        return new LaptopRecordEngine(
            new List<ILaptopRecordAdjuster>
            {
                new PercentLaptopRecordAdjuster(-19.9),
                new FlaggedLaptopRecordAdjuster(flags, "laptop-record-batched", 8.0),
                new RegionLaptopRecordAdjuster(new Dictionary<string, double> { ["west"] = 0.99 }),
                new TieredLaptopRecordAdjuster(
                    new List<double> { 7190.0, 7761.0, 8841.0 },
                    new List<double> { 0.95, 0.94, 1.0 }),
            },
            new List<string>(),
            0.4,
            85694.3);
    }

    public double Run(LaptopRecord item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
