namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient asset return adjustment calculation.</summary>
public interface ILenientAssetReturnAdjustmentModifier
{
    string Describe();
    double Adjust(LenientAssetReturnAdjustment item, double valueValue);
}

public sealed record LenientAssetReturnAdjustmentLimitOffsetAdjuster(double Weight) : ILenientAssetReturnAdjustmentModifier
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(LenientAssetReturnAdjustment item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record CapLenientAssetReturnAdjustmentAdjuster(double Cap) : ILenientAssetReturnAdjustmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientAssetReturnAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLenientAssetReturnAdjustmentAdjuster(double Percent) : ILenientAssetReturnAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientAssetReturnAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientAssetReturnAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : ILenientAssetReturnAdjustmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientAssetReturnAdjustment item, double valueValue)
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

public sealed record MinimumLenientAssetReturnAdjustmentAdjuster(double Minimum) : ILenientAssetReturnAdjustmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientAssetReturnAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the lenient asset return adjustment steps in order and keeps a trace of each.</summary>
public sealed class LenientAssetReturnAdjustmentEngine(List<ILenientAssetReturnAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientAssetReturnAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientAssetReturnAdjustmentEngine DefaultLenientAssetReturnAdjustmentEngine()
    {
        return new LenientAssetReturnAdjustmentEngine(
            new List<ILenientAssetReturnAdjustmentModifier>
            {
                new LenientAssetReturnAdjustmentLimitOffsetAdjuster(0.09),
                new CapLenientAssetReturnAdjustmentAdjuster(18850.0),
                new PercentLenientAssetReturnAdjustmentAdjuster(23.0),
                new TieredLenientAssetReturnAdjustmentAdjuster(
                    new List<double> { 5338.0, 5883.0, 7134.0 },
                    new List<double> { 1.09, 1.02, 0.99 }),
                new MinimumLenientAssetReturnAdjustmentAdjuster(19.3),
            },
            new List<string>(),
            4.9,
            78714.1);
    }

    public double Compute(LenientAssetReturnAdjustment item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientAssetReturnAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
