namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict asset return segment calculation.</summary>
public interface IStrictAssetReturnSegmentAdjuster
{
    string Describe();
    double Adjust(StrictAssetReturnSegment item, double valueValue);
}

public sealed record TieredStrictAssetReturnSegmentAdjuster(List<double> Bounds, List<double> Rates) : IStrictAssetReturnSegmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictAssetReturnSegment item, double valueValue)
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

public sealed record StrictAssetReturnSegmentQuantityOffsetAdjuster(double Weight) : IStrictAssetReturnSegmentAdjuster
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(StrictAssetReturnSegment item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record RoundingStrictAssetReturnSegmentAdjuster(double Step) : IStrictAssetReturnSegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictAssetReturnSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumStrictAssetReturnSegmentAdjuster(double Minimum) : IStrictAssetReturnSegmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictAssetReturnSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the strict asset return segment steps in order and keeps a trace of each.</summary>
public sealed class StrictAssetReturnSegmentEngine(List<IStrictAssetReturnSegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictAssetReturnSegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictAssetReturnSegmentEngine DefaultStrictAssetReturnSegmentEngine()
    {
        return new StrictAssetReturnSegmentEngine(
            new List<IStrictAssetReturnSegmentAdjuster>
            {
                new TieredStrictAssetReturnSegmentAdjuster(
                    new List<double> { 261.0, 359.0, 4381.0 },
                    new List<double> { 0.98, 0.92, 1.15 }),
                new StrictAssetReturnSegmentQuantityOffsetAdjuster(0.77),
                new RoundingStrictAssetReturnSegmentAdjuster(0.05),
                new MinimumStrictAssetReturnSegmentAdjuster(14.0),
            },
            new List<string>(),
            3.4,
            40143.0);
    }

    public double Price(StrictAssetReturnSegment item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<StrictAssetReturnSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
