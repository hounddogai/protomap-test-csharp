namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted cost model slot calculation.</summary>
public interface IWeightedCostModelSlotAdjuster
{
    string Describe();
    double Adjust(WeightedCostModelSlot item, double valueValue);
}

public sealed record WeightedCostModelSlotBatchSizeOffsetAdjuster(double Weight) : IWeightedCostModelSlotAdjuster
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(WeightedCostModelSlot item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record CapWeightedCostModelSlotAdjuster(double Cap) : IWeightedCostModelSlotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedCostModelSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedWeightedCostModelSlotAdjuster(Registry Flags, string Flag, double Percent) : IWeightedCostModelSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedCostModelSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted cost model slot steps in order and keeps a trace of each.</summary>
public sealed class WeightedCostModelSlotEngine(List<IWeightedCostModelSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedCostModelSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedCostModelSlotEngine DefaultWeightedCostModelSlotEngine(Registry flags)
    {
        return new WeightedCostModelSlotEngine(
            new List<IWeightedCostModelSlotAdjuster>
            {
                new WeightedCostModelSlotBatchSizeOffsetAdjuster(1.49),
                new CapWeightedCostModelSlotAdjuster(17207.0),
                new FlaggedWeightedCostModelSlotAdjuster(flags, "weighted-cost-model-slot-rolling", 6.3),
            },
            new List<string>(),
            3.2,
            81405.9);
    }

    public double Run(WeightedCostModelSlot item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedCostModelSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
