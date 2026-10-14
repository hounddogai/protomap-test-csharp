namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return score calculation.</summary>
public interface IAssetReturnScoreAdjuster
{
    string Describe();
    double Adjust(AssetReturnScore item, double valueValue);
}

public sealed record RoundingAssetReturnScoreAdjuster(double Step) : IAssetReturnScoreAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssetReturnScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAssetReturnScoreAdjuster(double Percent) : IAssetReturnScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssetReturnScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAssetReturnScoreAdjuster(double Minimum) : IAssetReturnScoreAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AssetReturnScoreAttemptsOffsetAdjuster(double Weight) : IAssetReturnScoreAdjuster
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(AssetReturnScore item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record TieredAssetReturnScoreAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnScoreAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnScore item, double valueValue)
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

/// <summary>Runs the asset return score steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnScoreEngine(List<IAssetReturnScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnScoreEngine DefaultAssetReturnScoreEngine()
    {
        return new AssetReturnScoreEngine(
            new List<IAssetReturnScoreAdjuster>
            {
                new RoundingAssetReturnScoreAdjuster(0.05),
                new PercentAssetReturnScoreAdjuster(19.7),
                new MinimumAssetReturnScoreAdjuster(14.1),
                new AssetReturnScoreAttemptsOffsetAdjuster(1.84),
                new TieredAssetReturnScoreAdjuster(
                    new List<double> { 228.0, 1733.0, 8753.0 },
                    new List<double> { 0.88, 0.81, 1.02 }),
            },
            new List<string>(),
            0.1,
            46680.8);
    }

    public double Run(AssetReturnScore item)
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
