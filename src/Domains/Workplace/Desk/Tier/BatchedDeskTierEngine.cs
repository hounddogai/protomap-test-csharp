namespace Acme.Workforce.Workplace.Desk.Tier;

/// <summary>One step of the batched desk tier calculation.</summary>
public interface IBatchedDeskTierFactor
{
    string Describe();
    double Adjust(BatchedDeskTier item, double valueValue);
}

public sealed record CapBatchedDeskTierAdjuster(double Cap) : IBatchedDeskTierFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedDeskTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredBatchedDeskTierAdjuster(List<double> Bounds, List<double> Rates) : IBatchedDeskTierFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedDeskTier item, double valueValue)
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

public sealed record CountryBatchedDeskTierAdjuster(Dictionary<string, double> Factors) : IBatchedDeskTierFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(BatchedDeskTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the batched desk tier steps in order and keeps a trace of each.</summary>
public sealed class BatchedDeskTierEngine(List<IBatchedDeskTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedDeskTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedDeskTierEngine DefaultBatchedDeskTierEngine()
    {
        return new BatchedDeskTierEngine(
            new List<IBatchedDeskTierFactor>
            {
                new CapBatchedDeskTierAdjuster(33717.0),
                new TieredBatchedDeskTierAdjuster(
                    new List<double> { 1191.0, 5561.0, 8788.0 },
                    new List<double> { 1.01, 1.2, 0.8 }),
                new CountryBatchedDeskTierAdjuster(new Dictionary<string, double> { ["online"] = 0.86 }),
            },
            new List<string>(),
            3.5,
            29123.3);
    }

    public double Compute(BatchedDeskTier item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedDeskTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
