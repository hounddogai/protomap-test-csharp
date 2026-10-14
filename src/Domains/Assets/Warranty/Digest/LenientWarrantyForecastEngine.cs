namespace Acme.Workforce.Assets.Warranty.Digest;

/// <summary>One step of the lenient warranty forecast calculation.</summary>
public interface ILenientWarrantyForecastStep
{
    string Describe();
    double Adjust(LenientWarrantyForecast item, double valueValue);
}

public sealed record TieredLenientWarrantyForecastAdjuster(List<double> Bounds, List<double> Rates) : ILenientWarrantyForecastStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientWarrantyForecast item, double valueValue)
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

public sealed record LenientWarrantyForecastAttemptsOffsetAdjuster(double Weight) : ILenientWarrantyForecastStep
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(LenientWarrantyForecast item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record MinimumLenientWarrantyForecastAdjuster(double Minimum) : ILenientWarrantyForecastStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientWarrantyForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLenientWarrantyForecastAdjuster(double Cap) : ILenientWarrantyForecastStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientWarrantyForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLenientWarrantyForecastAdjuster(double Percent) : ILenientWarrantyForecastStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientWarrantyForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelLenientWarrantyForecastAdjuster(Dictionary<string, double> Factors) : ILenientWarrantyForecastStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LenientWarrantyForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient warranty forecast steps in order and keeps a trace of each.</summary>
public sealed class LenientWarrantyForecastEngine(List<ILenientWarrantyForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientWarrantyForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientWarrantyForecastEngine DefaultLenientWarrantyForecastEngine()
    {
        return new LenientWarrantyForecastEngine(
            new List<ILenientWarrantyForecastStep>
            {
                new TieredLenientWarrantyForecastAdjuster(
                    new List<double> { 3430.0, 6445.0 },
                    new List<double> { 0.88, 1.09 }),
                new LenientWarrantyForecastAttemptsOffsetAdjuster(1.77),
                new MinimumLenientWarrantyForecastAdjuster(30.1),
                new CapLenientWarrantyForecastAdjuster(43461.0),
                new PercentLenientWarrantyForecastAdjuster(15.3),
                new LabelLenientWarrantyForecastAdjuster(new Dictionary<string, double> { ["south"] = 1.29 }),
            },
            new List<string>(),
            1.6,
            65890.3);
    }

    public double Evaluate(LenientWarrantyForecast item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientWarrantyForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
