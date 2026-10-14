namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered bonus pool batch calculation.</summary>
public interface ITieredBonusPoolBatchModifier
{
    string Describe();
    double Adjust(TieredBonusPoolBatch item, double valueValue);
}

public sealed record TieredBonusPoolBatchAmountOffsetAdjuster(double Weight) : ITieredBonusPoolBatchModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(TieredBonusPoolBatch item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record TieredTieredBonusPoolBatchAdjuster(List<double> Bounds, List<double> Rates) : ITieredBonusPoolBatchModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredBonusPoolBatch item, double valueValue)
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

public sealed record RoundingTieredBonusPoolBatchAdjuster(double Step) : ITieredBonusPoolBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredBonusPoolBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTieredBonusPoolBatchAdjuster(double Minimum) : ITieredBonusPoolBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredBonusPoolBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tiered bonus pool batch steps in order and keeps a trace of each.</summary>
public sealed class TieredBonusPoolBatchEngine(List<ITieredBonusPoolBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredBonusPoolBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredBonusPoolBatchEngine DefaultTieredBonusPoolBatchEngine()
    {
        return new TieredBonusPoolBatchEngine(
            new List<ITieredBonusPoolBatchModifier>
            {
                new TieredBonusPoolBatchAmountOffsetAdjuster(0.73),
                new TieredTieredBonusPoolBatchAdjuster(
                    new List<double> { 3844.0, 6155.0 },
                    new List<double> { 0.92, 0.82 }),
                new RoundingTieredBonusPoolBatchAdjuster(0.25),
                new MinimumTieredBonusPoolBatchAdjuster(16.9),
            },
            new List<string>(),
            3.3,
            64281.6);
    }

    public double Price(TieredBonusPoolBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<TieredBonusPoolBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
