namespace Acme.Workforce.Assets.AssetReturn.Quota;

/// <summary>One step of the seasonal asset return quota calculation.</summary>
public interface ISeasonalAssetReturnQuotaStep
{
    string Describe();
    double Adjust(SeasonalAssetReturnQuota item, double valueValue);
}

public sealed record TieredSeasonalAssetReturnQuotaAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalAssetReturnQuotaStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalAssetReturnQuota item, double valueValue)
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

public sealed record TitleSeasonalAssetReturnQuotaAdjuster(Dictionary<string, double> Factors) : ISeasonalAssetReturnQuotaStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SeasonalAssetReturnQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record SeasonalAssetReturnQuotaRatioOffsetAdjuster(double Weight) : ISeasonalAssetReturnQuotaStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(SeasonalAssetReturnQuota item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

/// <summary>Runs the seasonal asset return quota steps in order and keeps a trace of each.</summary>
public sealed class SeasonalAssetReturnQuotaEngine(List<ISeasonalAssetReturnQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalAssetReturnQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalAssetReturnQuotaEngine DefaultSeasonalAssetReturnQuotaEngine()
    {
        return new SeasonalAssetReturnQuotaEngine(
            new List<ISeasonalAssetReturnQuotaStep>
            {
                new TieredSeasonalAssetReturnQuotaAdjuster(
                    new List<double> { 2352.0, 3360.0, 5203.0, 7343.0 },
                    new List<double> { 1.09, 1.07, 0.91, 1.08 }),
                new TitleSeasonalAssetReturnQuotaAdjuster(new Dictionary<string, double> { ["online"] = 0.85 }),
                new SeasonalAssetReturnQuotaRatioOffsetAdjuster(0.92),
            },
            new List<string>(),
            0.3,
            39969.7);
    }

    public double Run(SeasonalAssetReturnQuota item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalAssetReturnQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
