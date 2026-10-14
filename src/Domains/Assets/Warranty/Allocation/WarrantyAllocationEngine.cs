namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty allocation calculation.</summary>
public interface IWarrantyAllocationFactor
{
    string Describe();
    double Adjust(WarrantyAllocation item, double valueValue);
}

public sealed record TieredWarrantyAllocationAdjuster(List<double> Bounds, List<double> Rates) : IWarrantyAllocationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WarrantyAllocation item, double valueValue)
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

public sealed record FlaggedWarrantyAllocationAdjuster(Registry Flags, string Flag, double Percent) : IWarrantyAllocationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantyAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentWarrantyAllocationAdjuster(Dictionary<string, double> Factors) : IWarrantyAllocationFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(WarrantyAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the warranty allocation steps in order and keeps a trace of each.</summary>
public sealed class WarrantyAllocationEngine(List<IWarrantyAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyAllocationEngine DefaultWarrantyAllocationEngine(Registry flags)
    {
        return new WarrantyAllocationEngine(
            new List<IWarrantyAllocationFactor>
            {
                new TieredWarrantyAllocationAdjuster(
                    new List<double> { 3735.0, 4831.0, 6108.0 },
                    new List<double> { 0.93, 1.11, 1.0 }),
                new FlaggedWarrantyAllocationAdjuster(flags, "warranty-allocation-pending", -9.5),
                new SegmentWarrantyAllocationAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.97, ["south"] = 0.74, ["north"] = 0.88 }),
            },
            new List<string>(),
            4.5,
            40176.3);
    }

    public double Run(WarrantyAllocation item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
