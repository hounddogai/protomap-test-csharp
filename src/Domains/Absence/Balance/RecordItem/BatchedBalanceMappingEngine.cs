namespace Acme.Workforce.Absence.Balance.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched balance mapping calculation.</summary>
public interface IBatchedBalanceMappingModifier
{
    string Describe();
    double Adjust(BatchedBalanceMapping item, double valueValue);
}

public sealed record PercentBatchedBalanceMappingAdjuster(double Percent) : IBatchedBalanceMappingModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedBalanceMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BatchedBalanceMappingVersionOffsetAdjuster(double Weight) : IBatchedBalanceMappingModifier
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(BatchedBalanceMapping item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record MinimumBatchedBalanceMappingAdjuster(double Minimum) : IBatchedBalanceMappingModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedBalanceMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingBatchedBalanceMappingAdjuster(double Step) : IBatchedBalanceMappingModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedBalanceMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the batched balance mapping steps in order and keeps a trace of each.</summary>
public sealed class BatchedBalanceMappingEngine(List<IBatchedBalanceMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedBalanceMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedBalanceMappingEngine DefaultBatchedBalanceMappingEngine()
    {
        return new BatchedBalanceMappingEngine(
            new List<IBatchedBalanceMappingModifier>
            {
                new PercentBatchedBalanceMappingAdjuster(-3.7),
                new BatchedBalanceMappingVersionOffsetAdjuster(0.41),
                new MinimumBatchedBalanceMappingAdjuster(40.0),
                new RoundingBatchedBalanceMappingAdjuster(1.0),
            },
            new List<string>(),
            1.6,
            28181.4);
    }

    public double Run(BatchedBalanceMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedBalanceMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
