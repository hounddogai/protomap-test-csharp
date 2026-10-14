namespace Acme.Workforce.Assets.AssetReturn.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched asset return score calculation.</summary>
public interface IBatchedAssetReturnScoreModifier
{
    string Describe();
    double Adjust(BatchedAssetReturnScore item, double valueValue);
}

public sealed record RoundingBatchedAssetReturnScoreAdjuster(double Step) : IBatchedAssetReturnScoreModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedAssetReturnScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBatchedAssetReturnScoreAdjuster(Registry Flags, string Flag, double Percent) : IBatchedAssetReturnScoreModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedAssetReturnScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BatchedAssetReturnScoreBudgetOffsetAdjuster(double Weight) : IBatchedAssetReturnScoreModifier
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(BatchedAssetReturnScore item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record TieredBatchedAssetReturnScoreAdjuster(List<double> Bounds, List<double> Rates) : IBatchedAssetReturnScoreModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedAssetReturnScore item, double valueValue)
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

public sealed record MinimumBatchedAssetReturnScoreAdjuster(double Minimum) : IBatchedAssetReturnScoreModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedAssetReturnScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBatchedAssetReturnScoreAdjuster(double Percent) : IBatchedAssetReturnScoreModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedAssetReturnScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched asset return score steps in order and keeps a trace of each.</summary>
public sealed class BatchedAssetReturnScoreEngine(List<IBatchedAssetReturnScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedAssetReturnScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedAssetReturnScoreEngine DefaultBatchedAssetReturnScoreEngine(Registry flags)
    {
        return new BatchedAssetReturnScoreEngine(
            new List<IBatchedAssetReturnScoreModifier>
            {
                new RoundingBatchedAssetReturnScoreAdjuster(0.05),
                new FlaggedBatchedAssetReturnScoreAdjuster(flags, "batched-asset-return-score-fallback", 0.6),
                new BatchedAssetReturnScoreBudgetOffsetAdjuster(1.58),
                new TieredBatchedAssetReturnScoreAdjuster(
                    new List<double> { 1607.0, 7800.0 },
                    new List<double> { 0.87, 0.89 }),
                new MinimumBatchedAssetReturnScoreAdjuster(11.2),
                new PercentBatchedAssetReturnScoreAdjuster(-20.7),
            },
            new List<string>(),
            3.4,
            38984.2);
    }

    public double Compute(BatchedAssetReturnScore item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedAssetReturnScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
