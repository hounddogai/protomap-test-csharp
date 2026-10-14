namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return forecast calculation.</summary>
public interface IAssetReturnForecastStep
{
    string Describe();
    double Adjust(AssetReturnForecast item, double valueValue);
}

public sealed record RoundingAssetReturnForecastAdjuster(double Step) : IAssetReturnForecastStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssetReturnForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAssetReturnForecastAdjuster(double Minimum) : IAssetReturnForecastStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAssetReturnForecastAdjuster(Registry Flags, string Flag, double Percent) : IAssetReturnForecastStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssetReturnForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAssetReturnForecastAdjuster(double Cap) : IAssetReturnForecastStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssetReturnForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the asset return forecast steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnForecastEngine(List<IAssetReturnForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnForecastEngine DefaultAssetReturnForecastEngine(Registry flags)
    {
        return new AssetReturnForecastEngine(
            new List<IAssetReturnForecastStep>
            {
                new RoundingAssetReturnForecastAdjuster(0.25),
                new MinimumAssetReturnForecastAdjuster(38.5),
                new FlaggedAssetReturnForecastAdjuster(flags, "asset-return-forecast-regional", -4.9),
                new CapAssetReturnForecastAdjuster(2545.0),
            },
            new List<string>(),
            4.5,
            33302.3);
    }

    public double Price(AssetReturnForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
