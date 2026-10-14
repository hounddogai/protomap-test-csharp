namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted asset return forecast calculation.</summary>
public interface IWeightedAssetReturnForecastModifier
{
    string Describe();
    double Adjust(WeightedAssetReturnForecast item, double valueValue);
}

public sealed record TieredWeightedAssetReturnForecastAdjuster(List<double> Bounds, List<double> Rates) : IWeightedAssetReturnForecastModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedAssetReturnForecast item, double valueValue)
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

public sealed record RoundingWeightedAssetReturnForecastAdjuster(double Step) : IWeightedAssetReturnForecastModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedAssetReturnForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentWeightedAssetReturnForecastAdjuster(double Percent) : IWeightedAssetReturnForecastModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedAssetReturnForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted asset return forecast steps in order and keeps a trace of each.</summary>
public sealed class WeightedAssetReturnForecastEngine(List<IWeightedAssetReturnForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAssetReturnForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAssetReturnForecastEngine DefaultWeightedAssetReturnForecastEngine()
    {
        return new WeightedAssetReturnForecastEngine(
            new List<IWeightedAssetReturnForecastModifier>
            {
                new TieredWeightedAssetReturnForecastAdjuster(
                    new List<double> { 1804.0, 1806.0, 2932.0, 6666.0 },
                    new List<double> { 1.05, 1.19, 1.18, 0.86 }),
                new RoundingWeightedAssetReturnForecastAdjuster(0.05),
                new PercentWeightedAssetReturnForecastAdjuster(-15.5),
            },
            new List<string>(),
            3.5,
            32154.1);
    }

    public double Price(WeightedAssetReturnForecast item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAssetReturnForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
