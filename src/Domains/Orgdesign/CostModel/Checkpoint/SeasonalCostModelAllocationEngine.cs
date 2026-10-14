namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal cost model allocation calculation.</summary>
public interface ISeasonalCostModelAllocationModifier
{
    string Describe();
    double Adjust(SeasonalCostModelAllocation item, double valueValue);
}

public sealed record TieredSeasonalCostModelAllocationAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalCostModelAllocationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalCostModelAllocation item, double valueValue)
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

public sealed record FlaggedSeasonalCostModelAllocationAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalCostModelAllocationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalCostModelAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalCostModelAllocationAdjuster(double Cap) : ISeasonalCostModelAllocationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalCostModelAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SourceSeasonalCostModelAllocationAdjuster(Dictionary<string, double> Factors) : ISeasonalCostModelAllocationModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(SeasonalCostModelAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSeasonalCostModelAllocationAdjuster(double Step) : ISeasonalCostModelAllocationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalCostModelAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the seasonal cost model allocation steps in order and keeps a trace of each.</summary>
public sealed class SeasonalCostModelAllocationEngine(List<ISeasonalCostModelAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalCostModelAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalCostModelAllocationEngine DefaultSeasonalCostModelAllocationEngine(Registry flags)
    {
        return new SeasonalCostModelAllocationEngine(
            new List<ISeasonalCostModelAllocationModifier>
            {
                new TieredSeasonalCostModelAllocationAdjuster(
                    new List<double> { 3134.0, 3458.0, 5787.0, 6443.0 },
                    new List<double> { 1.02, 1.11, 0.99, 1.01 }),
                new FlaggedSeasonalCostModelAllocationAdjuster(flags, "seasonal-cost-model-allocation-legacy", -9.7),
                new CapSeasonalCostModelAllocationAdjuster(42062.0),
                new SourceSeasonalCostModelAllocationAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.06, ["east"] = 1.16 }),
                new RoundingSeasonalCostModelAllocationAdjuster(1.0),
            },
            new List<string>(),
            1.1,
            86932.7);
    }

    public double Price(SeasonalCostModelAllocation item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalCostModelAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
