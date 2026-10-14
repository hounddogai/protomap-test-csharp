namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the cost model checkpoint calculation.</summary>
public interface ICostModelCheckpointAdjuster
{
    string Describe();
    double Adjust(CostModelCheckpoint item, double valueValue);
}

public sealed record CostModelCheckpointScoreOffsetAdjuster(double Weight) : ICostModelCheckpointAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(CostModelCheckpoint item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record FlaggedCostModelCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ICostModelCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameCostModelCheckpointAdjuster(Dictionary<string, double> Factors) : ICostModelCheckpointAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(CostModelCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredCostModelCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ICostModelCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelCheckpoint item, double valueValue)
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

public sealed record PercentCostModelCheckpointAdjuster(double Percent) : ICostModelCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCostModelCheckpointAdjuster(double Cap) : ICostModelCheckpointAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CostModelCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the cost model checkpoint steps in order and keeps a trace of each.</summary>
public sealed class CostModelCheckpointEngine(List<ICostModelCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelCheckpointEngine DefaultCostModelCheckpointEngine(Registry flags)
    {
        return new CostModelCheckpointEngine(
            new List<ICostModelCheckpointAdjuster>
            {
                new CostModelCheckpointScoreOffsetAdjuster(1.47),
                new FlaggedCostModelCheckpointAdjuster(flags, "cost-model-checkpoint-tiered", 2.9),
                new NameCostModelCheckpointAdjuster(new Dictionary<string, double> { ["north"] = 1.06 }),
                new TieredCostModelCheckpointAdjuster(
                    new List<double> { 292.0, 348.0, 964.0, 8381.0 },
                    new List<double> { 0.93, 0.91, 0.91, 1.12 }),
                new PercentCostModelCheckpointAdjuster(-1.7),
                new CapCostModelCheckpointAdjuster(23337.0),
            },
            new List<string>(),
            2.2,
            97007.6);
    }

    public double Run(CostModelCheckpoint item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
