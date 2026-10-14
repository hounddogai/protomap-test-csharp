namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>One step of the batched position estimate calculation.</summary>
public interface IBatchedPositionEstimateModifier
{
    string Describe();
    double Adjust(BatchedPositionEstimate item, double valueValue);
}

public sealed record TieredBatchedPositionEstimateAdjuster(List<double> Bounds, List<double> Rates) : IBatchedPositionEstimateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedPositionEstimate item, double valueValue)
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

public sealed record MinimumBatchedPositionEstimateAdjuster(double Minimum) : IBatchedPositionEstimateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedPositionEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBatchedPositionEstimateAdjuster(double Cap) : IBatchedPositionEstimateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedPositionEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentBatchedPositionEstimateAdjuster(double Percent) : IBatchedPositionEstimateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedPositionEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameBatchedPositionEstimateAdjuster(Dictionary<string, double> Factors) : IBatchedPositionEstimateModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BatchedPositionEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record BatchedPositionEstimateMinimumOffsetAdjuster(double Weight) : IBatchedPositionEstimateModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(BatchedPositionEstimate item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

/// <summary>Runs the batched position estimate steps in order and keeps a trace of each.</summary>
public sealed class BatchedPositionEstimateEngine(List<IBatchedPositionEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedPositionEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedPositionEstimateEngine DefaultBatchedPositionEstimateEngine()
    {
        return new BatchedPositionEstimateEngine(
            new List<IBatchedPositionEstimateModifier>
            {
                new TieredBatchedPositionEstimateAdjuster(
                    new List<double> { 4105.0, 4441.0, 6336.0 },
                    new List<double> { 1.13, 1.03, 1.05 }),
                new MinimumBatchedPositionEstimateAdjuster(7.7),
                new CapBatchedPositionEstimateAdjuster(38769.0),
                new PercentBatchedPositionEstimateAdjuster(-23.0),
                new NameBatchedPositionEstimateAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.79, ["north"] = 1.26, ["retail"] = 1.22 }),
                new BatchedPositionEstimateMinimumOffsetAdjuster(0.36),
            },
            new List<string>(),
            4.4,
            12451.7);
    }

    public double Compute(BatchedPositionEstimate item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedPositionEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
