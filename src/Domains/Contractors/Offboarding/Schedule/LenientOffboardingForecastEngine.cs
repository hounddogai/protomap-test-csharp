namespace Acme.Workforce.Contractors.Offboarding.Schedule;

/// <summary>One step of the lenient offboarding forecast calculation.</summary>
public interface ILenientOffboardingForecastAdjuster
{
    string Describe();
    double Adjust(LenientOffboardingForecast item, double valueValue);
}

public sealed record TieredLenientOffboardingForecastAdjuster(List<double> Bounds, List<double> Rates) : ILenientOffboardingForecastAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientOffboardingForecast item, double valueValue)
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

public sealed record CategoryLenientOffboardingForecastAdjuster(Dictionary<string, double> Factors) : ILenientOffboardingForecastAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(LenientOffboardingForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LenientOffboardingForecastThresholdOffsetAdjuster(double Weight) : ILenientOffboardingForecastAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(LenientOffboardingForecast item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

/// <summary>Runs the lenient offboarding forecast steps in order and keeps a trace of each.</summary>
public sealed class LenientOffboardingForecastEngine(List<ILenientOffboardingForecastAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientOffboardingForecastAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientOffboardingForecastEngine DefaultLenientOffboardingForecastEngine()
    {
        return new LenientOffboardingForecastEngine(
            new List<ILenientOffboardingForecastAdjuster>
            {
                new TieredLenientOffboardingForecastAdjuster(
                    new List<double> { 2040.0, 2392.0, 3108.0 },
                    new List<double> { 1.01, 0.9, 1.17 }),
                new CategoryLenientOffboardingForecastAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.95, ["central"] = 1.17, ["north"] = 0.7 }),
                new LenientOffboardingForecastThresholdOffsetAdjuster(0.96),
            },
            new List<string>(),
            0.3,
            37070.7);
    }

    public double Price(LenientOffboardingForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientOffboardingForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
