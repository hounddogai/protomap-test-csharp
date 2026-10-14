namespace Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>One step of the adaptive asset return allocation calculation.</summary>
public interface IAdaptiveAssetReturnAllocationFactor
{
    string Describe();
    double Adjust(AdaptiveAssetReturnAllocation item, double valueValue);
}

public sealed record TieredAdaptiveAssetReturnAllocationAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveAssetReturnAllocationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveAssetReturnAllocation item, double valueValue)
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

public sealed record PercentAdaptiveAssetReturnAllocationAdjuster(double Percent) : IAdaptiveAssetReturnAllocationFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveAssetReturnAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdaptiveAssetReturnAllocationBatchSizeOffsetAdjuster(double Weight) : IAdaptiveAssetReturnAllocationFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(AdaptiveAssetReturnAllocation item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record MinimumAdaptiveAssetReturnAllocationAdjuster(double Minimum) : IAdaptiveAssetReturnAllocationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveAssetReturnAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive asset return allocation steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveAssetReturnAllocationEngine(List<IAdaptiveAssetReturnAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveAssetReturnAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveAssetReturnAllocationEngine DefaultAdaptiveAssetReturnAllocationEngine()
    {
        return new AdaptiveAssetReturnAllocationEngine(
            new List<IAdaptiveAssetReturnAllocationFactor>
            {
                new TieredAdaptiveAssetReturnAllocationAdjuster(
                    new List<double> { 1633.0, 2509.0, 6010.0, 6205.0 },
                    new List<double> { 0.92, 0.99, 0.9, 0.95 }),
                new PercentAdaptiveAssetReturnAllocationAdjuster(-6.6),
                new AdaptiveAssetReturnAllocationBatchSizeOffsetAdjuster(0.41),
                new MinimumAdaptiveAssetReturnAllocationAdjuster(27.3),
            },
            new List<string>(),
            1.8,
            72078.6);
    }

    public double Compute(AdaptiveAssetReturnAllocation item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveAssetReturnAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
