namespace Acme.Workforce.Payrollops.RetroPay.PayPolicy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched retro pay plan calculation.</summary>
public interface IBatchedRetroPayPlanFactor
{
    string Describe();
    double Adjust(BatchedRetroPayPlan item, double valueValue);
}

public sealed record TieredBatchedRetroPayPlanAdjuster(List<double> Bounds, List<double> Rates) : IBatchedRetroPayPlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedRetroPayPlan item, double valueValue)
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

public sealed record CapBatchedRetroPayPlanAdjuster(double Cap) : IBatchedRetroPayPlanFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedRetroPayPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentBatchedRetroPayPlanAdjuster(double Percent) : IBatchedRetroPayPlanFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedRetroPayPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched retro pay plan steps in order and keeps a trace of each.</summary>
public sealed class BatchedRetroPayPlanEngine(List<IBatchedRetroPayPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedRetroPayPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedRetroPayPlanEngine DefaultBatchedRetroPayPlanEngine()
    {
        return new BatchedRetroPayPlanEngine(
            new List<IBatchedRetroPayPlanFactor>
            {
                new TieredBatchedRetroPayPlanAdjuster(
                    new List<double> { 624.0, 3795.0, 5212.0, 8802.0 },
                    new List<double> { 0.98, 0.9, 1.08, 0.98 }),
                new CapBatchedRetroPayPlanAdjuster(17490.0),
                new PercentBatchedRetroPayPlanAdjuster(-7.4),
            },
            new List<string>(),
            1.4,
            79441.0);
    }

    public double Compute(BatchedRetroPayPlan item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedRetroPayPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
