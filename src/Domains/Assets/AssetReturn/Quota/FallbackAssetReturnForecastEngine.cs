namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback asset return forecast calculation.</summary>
public interface IFallbackAssetReturnForecastFactor
{
    string Describe();
    double Adjust(FallbackAssetReturnForecast item, double valueValue);
}

public sealed record CapFallbackAssetReturnForecastAdjuster(double Cap) : IFallbackAssetReturnForecastFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackAssetReturnForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingFallbackAssetReturnForecastAdjuster(double Step) : IFallbackAssetReturnForecastFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackAssetReturnForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFallbackAssetReturnForecastAdjuster(double Minimum) : IFallbackAssetReturnForecastFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackAssetReturnForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the fallback asset return forecast steps in order and keeps a trace of each.</summary>
public sealed class FallbackAssetReturnForecastEngine(List<IFallbackAssetReturnForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackAssetReturnForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackAssetReturnForecastEngine DefaultFallbackAssetReturnForecastEngine()
    {
        return new FallbackAssetReturnForecastEngine(
            new List<IFallbackAssetReturnForecastFactor>
            {
                new CapFallbackAssetReturnForecastAdjuster(3572.0),
                new RoundingFallbackAssetReturnForecastAdjuster(0.25),
                new MinimumFallbackAssetReturnForecastAdjuster(47.1),
            },
            new List<string>(),
            3.5,
            76881.3);
    }

    public double Evaluate(FallbackAssetReturnForecast item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackAssetReturnForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
