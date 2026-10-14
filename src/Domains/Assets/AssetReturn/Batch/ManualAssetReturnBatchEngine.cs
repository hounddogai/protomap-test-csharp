namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual asset return batch calculation.</summary>
public interface IManualAssetReturnBatchModifier
{
    string Describe();
    double Adjust(ManualAssetReturnBatch item, double valueValue);
}

public sealed record MinimumManualAssetReturnBatchAdjuster(double Minimum) : IManualAssetReturnBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualAssetReturnBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelManualAssetReturnBatchAdjuster(Dictionary<string, double> Factors) : IManualAssetReturnBatchModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ManualAssetReturnBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredManualAssetReturnBatchAdjuster(List<double> Bounds, List<double> Rates) : IManualAssetReturnBatchModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualAssetReturnBatch item, double valueValue)
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

public sealed record RoundingManualAssetReturnBatchAdjuster(double Step) : IManualAssetReturnBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualAssetReturnBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentManualAssetReturnBatchAdjuster(double Percent) : IManualAssetReturnBatchModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualAssetReturnBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual asset return batch steps in order and keeps a trace of each.</summary>
public sealed class ManualAssetReturnBatchEngine(List<IManualAssetReturnBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualAssetReturnBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualAssetReturnBatchEngine DefaultManualAssetReturnBatchEngine()
    {
        return new ManualAssetReturnBatchEngine(
            new List<IManualAssetReturnBatchModifier>
            {
                new MinimumManualAssetReturnBatchAdjuster(24.3),
                new LabelManualAssetReturnBatchAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.72, ["west"] = 1.03 }),
                new TieredManualAssetReturnBatchAdjuster(
                    new List<double> { 1362.0, 3787.0, 4715.0 },
                    new List<double> { 0.96, 1.11, 1.09 }),
                new RoundingManualAssetReturnBatchAdjuster(0.05),
                new PercentManualAssetReturnBatchAdjuster(-9.5),
            },
            new List<string>(),
            0.0,
            58869.7);
    }

    public double Evaluate(ManualAssetReturnBatch item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ManualAssetReturnBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
