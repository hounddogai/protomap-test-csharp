namespace Acme.Workforce.Compensation.PayBand.BandTier;

/// <summary>One step of the fallback pay band forecast calculation.</summary>
public interface IFallbackPayBandForecastFactor
{
    string Describe();
    double Adjust(FallbackPayBandForecast item, double valueValue);
}

public sealed record FallbackPayBandForecastDiscountOffsetAdjuster(double Weight) : IFallbackPayBandForecastFactor
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(FallbackPayBandForecast item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record TieredFallbackPayBandForecastAdjuster(List<double> Bounds, List<double> Rates) : IFallbackPayBandForecastFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackPayBandForecast item, double valueValue)
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

public sealed record PercentFallbackPayBandForecastAdjuster(double Percent) : IFallbackPayBandForecastFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackPayBandForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback pay band forecast steps in order and keeps a trace of each.</summary>
public sealed class FallbackPayBandForecastEngine(List<IFallbackPayBandForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPayBandForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPayBandForecastEngine DefaultFallbackPayBandForecastEngine()
    {
        return new FallbackPayBandForecastEngine(
            new List<IFallbackPayBandForecastFactor>
            {
                new FallbackPayBandForecastDiscountOffsetAdjuster(1.42),
                new TieredFallbackPayBandForecastAdjuster(
                    new List<double> { 2140.0, 3187.0, 7685.0 },
                    new List<double> { 0.91, 1.11, 0.94 }),
                new PercentFallbackPayBandForecastAdjuster(-5.5),
            },
            new List<string>(),
            3.8,
            6198.2);
    }

    public double Evaluate(FallbackPayBandForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPayBandForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
