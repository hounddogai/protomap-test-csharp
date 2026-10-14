namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default asset return estimate calculation.</summary>
public interface IDefaultAssetReturnEstimateAdjuster
{
    string Describe();
    double Adjust(DefaultAssetReturnEstimate item, double valueValue);
}

public sealed record CapDefaultAssetReturnEstimateAdjuster(double Cap) : IDefaultAssetReturnEstimateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultAssetReturnEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DefaultAssetReturnEstimateRatingOffsetAdjuster(double Weight) : IDefaultAssetReturnEstimateAdjuster
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(DefaultAssetReturnEstimate item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record PercentDefaultAssetReturnEstimateAdjuster(double Percent) : IDefaultAssetReturnEstimateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultAssetReturnEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleDefaultAssetReturnEstimateAdjuster(Dictionary<string, double> Factors) : IDefaultAssetReturnEstimateAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DefaultAssetReturnEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDefaultAssetReturnEstimateAdjuster(double Step) : IDefaultAssetReturnEstimateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultAssetReturnEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the default asset return estimate steps in order and keeps a trace of each.</summary>
public sealed class DefaultAssetReturnEstimateEngine(List<IDefaultAssetReturnEstimateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultAssetReturnEstimateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultAssetReturnEstimateEngine DefaultDefaultAssetReturnEstimateEngine()
    {
        return new DefaultAssetReturnEstimateEngine(
            new List<IDefaultAssetReturnEstimateAdjuster>
            {
                new CapDefaultAssetReturnEstimateAdjuster(18034.0),
                new DefaultAssetReturnEstimateRatingOffsetAdjuster(1.86),
                new PercentDefaultAssetReturnEstimateAdjuster(-2.4),
                new TitleDefaultAssetReturnEstimateAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.02, ["north"] = 0.71, ["online"] = 0.86 }),
                new RoundingDefaultAssetReturnEstimateAdjuster(5.0),
            },
            new List<string>(),
            0.9,
            87285.3);
    }

    public double Run(DefaultAssetReturnEstimate item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultAssetReturnEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
