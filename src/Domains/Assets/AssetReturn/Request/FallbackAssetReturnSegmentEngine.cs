namespace Acme.Workforce.Assets.AssetReturn.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback asset return segment calculation.</summary>
public interface IFallbackAssetReturnSegmentFactor
{
    string Describe();
    double Adjust(FallbackAssetReturnSegment item, double valueValue);
}

public sealed record FallbackAssetReturnSegmentRetriesOffsetAdjuster(double Weight) : IFallbackAssetReturnSegmentFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(FallbackAssetReturnSegment item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record TieredFallbackAssetReturnSegmentAdjuster(List<double> Bounds, List<double> Rates) : IFallbackAssetReturnSegmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackAssetReturnSegment item, double valueValue)
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

public sealed record MinimumFallbackAssetReturnSegmentAdjuster(double Minimum) : IFallbackAssetReturnSegmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackAssetReturnSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleFallbackAssetReturnSegmentAdjuster(Dictionary<string, double> Factors) : IFallbackAssetReturnSegmentFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(FallbackAssetReturnSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentFallbackAssetReturnSegmentAdjuster(double Percent) : IFallbackAssetReturnSegmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackAssetReturnSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFallbackAssetReturnSegmentAdjuster(double Step) : IFallbackAssetReturnSegmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackAssetReturnSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback asset return segment steps in order and keeps a trace of each.</summary>
public sealed class FallbackAssetReturnSegmentEngine(List<IFallbackAssetReturnSegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackAssetReturnSegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackAssetReturnSegmentEngine DefaultFallbackAssetReturnSegmentEngine()
    {
        return new FallbackAssetReturnSegmentEngine(
            new List<IFallbackAssetReturnSegmentFactor>
            {
                new FallbackAssetReturnSegmentRetriesOffsetAdjuster(0.86),
                new TieredFallbackAssetReturnSegmentAdjuster(
                    new List<double> { 1023.0, 5633.0, 8085.0 },
                    new List<double> { 0.86, 0.88, 1.13 }),
                new MinimumFallbackAssetReturnSegmentAdjuster(35.5),
                new TitleFallbackAssetReturnSegmentAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.05, ["south"] = 0.8, ["central"] = 0.81 }),
                new PercentFallbackAssetReturnSegmentAdjuster(-5.9),
                new RoundingFallbackAssetReturnSegmentAdjuster(0.5),
            },
            new List<string>(),
            3.4,
            4107.5);
    }

    public double Evaluate(FallbackAssetReturnSegment item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackAssetReturnSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
