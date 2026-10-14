namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg allocation calculation.</summary>
public interface IReorgAllocationAdjuster
{
    string Describe();
    double Adjust(ReorgAllocation item, double valueValue);
}

public sealed record RoundingReorgAllocationAdjuster(double Step) : IReorgAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedReorgAllocationAdjuster(Registry Flags, string Flag, double Percent) : IReorgAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredReorgAllocationAdjuster(List<double> Bounds, List<double> Rates) : IReorgAllocationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgAllocation item, double valueValue)
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

public sealed record NameReorgAllocationAdjuster(Dictionary<string, double> Factors) : IReorgAllocationAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ReorgAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the reorg allocation steps in order and keeps a trace of each.</summary>
public sealed class ReorgAllocationEngine(List<IReorgAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgAllocationEngine DefaultReorgAllocationEngine(Registry flags)
    {
        return new ReorgAllocationEngine(
            new List<IReorgAllocationAdjuster>
            {
                new RoundingReorgAllocationAdjuster(0.25),
                new FlaggedReorgAllocationAdjuster(flags, "reorg-allocation-tiered", 1.5),
                new TieredReorgAllocationAdjuster(
                    new List<double> { 1718.0, 8634.0, 9095.0, 9192.0 },
                    new List<double> { 1.13, 0.97, 1.18, 0.97 }),
                new NameReorgAllocationAdjuster(new Dictionary<string, double> { ["south"] = 0.98 }),
            },
            new List<string>(),
            4.2,
            38776.7);
    }

    public double Run(ReorgAllocation item)
    {
        var current = item.WeightedValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<ReorgAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
