namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty mapping calculation.</summary>
public interface IWarrantyMappingAdjuster
{
    string Describe();
    double Adjust(WarrantyMapping item, double valueValue);
}

public sealed record FlaggedWarrantyMappingAdjuster(Registry Flags, string Flag, double Percent) : IWarrantyMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantyMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWarrantyMappingAdjuster(List<double> Bounds, List<double> Rates) : IWarrantyMappingAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WarrantyMapping item, double valueValue)
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

public sealed record SegmentWarrantyMappingAdjuster(Dictionary<string, double> Factors) : IWarrantyMappingAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(WarrantyMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumWarrantyMappingAdjuster(double Minimum) : IWarrantyMappingAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record WarrantyMappingBatchSizeOffsetAdjuster(double Weight) : IWarrantyMappingAdjuster
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(WarrantyMapping item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

/// <summary>Runs the warranty mapping steps in order and keeps a trace of each.</summary>
public sealed class WarrantyMappingEngine(List<IWarrantyMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyMappingEngine DefaultWarrantyMappingEngine(Registry flags)
    {
        return new WarrantyMappingEngine(
            new List<IWarrantyMappingAdjuster>
            {
                new FlaggedWarrantyMappingAdjuster(flags, "warranty-mapping-rolling", -8.4),
                new TieredWarrantyMappingAdjuster(
                    new List<double> { 4003.0, 9305.0 },
                    new List<double> { 1.16, 1.13 }),
                new SegmentWarrantyMappingAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.35, ["retail"] = 1.1, ["central"] = 0.75 }),
                new MinimumWarrantyMappingAdjuster(36.6),
                new WarrantyMappingBatchSizeOffsetAdjuster(0.61),
            },
            new List<string>(),
            2.0,
            63422.7);
    }

    public double Price(WarrantyMapping item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
