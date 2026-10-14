namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return target calculation.</summary>
public interface IAssetReturnTargetAdjuster
{
    string Describe();
    double Adjust(AssetReturnTarget item, double valueValue);
}

public sealed record RoundingAssetReturnTargetAdjuster(double Step) : IAssetReturnTargetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssetReturnTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAssetReturnTargetAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnTargetAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnTarget item, double valueValue)
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

public sealed record PercentAssetReturnTargetAdjuster(double Percent) : IAssetReturnTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssetReturnTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAssetReturnTargetAdjuster(double Minimum) : IAssetReturnTargetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelAssetReturnTargetAdjuster(Dictionary<string, double> Factors) : IAssetReturnTargetAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AssetReturnTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the asset return target steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnTargetEngine(List<IAssetReturnTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnTargetEngine DefaultAssetReturnTargetEngine()
    {
        return new AssetReturnTargetEngine(
            new List<IAssetReturnTargetAdjuster>
            {
                new RoundingAssetReturnTargetAdjuster(5.0),
                new TieredAssetReturnTargetAdjuster(
                    new List<double> { 1722.0, 2751.0, 7885.0 },
                    new List<double> { 0.94, 1.0, 1.1 }),
                new PercentAssetReturnTargetAdjuster(3.7),
                new MinimumAssetReturnTargetAdjuster(49.0),
                new LabelAssetReturnTargetAdjuster(new Dictionary<string, double> { ["west"] = 1.22 }),
            },
            new List<string>(),
            3.6,
            99506.2);
    }

    public double Evaluate(AssetReturnTarget item)
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
