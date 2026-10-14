namespace Acme.Workforce.Engagement.TownHall.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched town hall quota calculation.</summary>
public interface IBatchedTownHallQuotaModifier
{
    string Describe();
    double Adjust(BatchedTownHallQuota item, double valueValue);
}

public sealed record BatchedTownHallQuotaSequenceOffsetAdjuster(double Weight) : IBatchedTownHallQuotaModifier
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(BatchedTownHallQuota item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record TieredBatchedTownHallQuotaAdjuster(List<double> Bounds, List<double> Rates) : IBatchedTownHallQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedTownHallQuota item, double valueValue)
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

public sealed record CapBatchedTownHallQuotaAdjuster(double Cap) : IBatchedTownHallQuotaModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedTownHallQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumBatchedTownHallQuotaAdjuster(double Minimum) : IBatchedTownHallQuotaModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedTownHallQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBatchedTownHallQuotaAdjuster(double Percent) : IBatchedTownHallQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedTownHallQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelBatchedTownHallQuotaAdjuster(Dictionary<string, double> Factors) : IBatchedTownHallQuotaModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BatchedTownHallQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the batched town hall quota steps in order and keeps a trace of each.</summary>
public sealed class BatchedTownHallQuotaEngine(List<IBatchedTownHallQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedTownHallQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedTownHallQuotaEngine DefaultBatchedTownHallQuotaEngine()
    {
        return new BatchedTownHallQuotaEngine(
            new List<IBatchedTownHallQuotaModifier>
            {
                new BatchedTownHallQuotaSequenceOffsetAdjuster(1.09),
                new TieredBatchedTownHallQuotaAdjuster(
                    new List<double> { 929.0, 5541.0, 8340.0, 9428.0 },
                    new List<double> { 0.8, 1.06, 0.95, 0.9 }),
                new CapBatchedTownHallQuotaAdjuster(24327.0),
                new MinimumBatchedTownHallQuotaAdjuster(40.6),
                new PercentBatchedTownHallQuotaAdjuster(21.9),
                new LabelBatchedTownHallQuotaAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.13, ["north"] = 0.91, ["south"] = 0.9 }),
            },
            new List<string>(),
            4.9,
            60190.8);
    }

    public double Run(BatchedTownHallQuota item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedTownHallQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
