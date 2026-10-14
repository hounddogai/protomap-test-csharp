namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped cost model checkpoint calculation.</summary>
public interface ICappedCostModelCheckpointStep
{
    string Describe();
    double Adjust(CappedCostModelCheckpoint item, double valueValue);
}

public sealed record CappedCostModelCheckpointUpdatedAtOffsetAdjuster(double Weight) : ICappedCostModelCheckpointStep
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(CappedCostModelCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record CapCappedCostModelCheckpointAdjuster(double Cap) : ICappedCostModelCheckpointStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedCostModelCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingCappedCostModelCheckpointAdjuster(double Step) : ICappedCostModelCheckpointStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedCostModelCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentCappedCostModelCheckpointAdjuster(double Percent) : ICappedCostModelCheckpointStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedCostModelCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedCappedCostModelCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ICappedCostModelCheckpointStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedCostModelCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedCostModelCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ICappedCostModelCheckpointStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedCostModelCheckpoint item, double valueValue)
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

/// <summary>Runs the capped cost model checkpoint steps in order and keeps a trace of each.</summary>
public sealed class CappedCostModelCheckpointEngine(List<ICappedCostModelCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedCostModelCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedCostModelCheckpointEngine DefaultCappedCostModelCheckpointEngine(Registry flags)
    {
        return new CappedCostModelCheckpointEngine(
            new List<ICappedCostModelCheckpointStep>
            {
                new CappedCostModelCheckpointUpdatedAtOffsetAdjuster(0.56),
                new CapCappedCostModelCheckpointAdjuster(44147.0),
                new RoundingCappedCostModelCheckpointAdjuster(0.5),
                new PercentCappedCostModelCheckpointAdjuster(22.8),
                new FlaggedCappedCostModelCheckpointAdjuster(flags, "capped-cost-model-checkpoint-pending", 2.2),
                new TieredCappedCostModelCheckpointAdjuster(
                    new List<double> { 1262.0, 9177.0 },
                    new List<double> { 1.13, 0.99 }),
            },
            new List<string>(),
            0.7,
            21262.9);
    }

    public double Evaluate(CappedCostModelCheckpoint item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedCostModelCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
