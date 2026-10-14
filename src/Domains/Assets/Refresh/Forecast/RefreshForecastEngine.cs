namespace Acme.Workforce.Assets.Refresh.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh forecast calculation.</summary>
public interface IRefreshForecastStep
{
    string Describe();
    double Adjust(RefreshForecast item, double valueValue);
}

public sealed record ChannelRefreshForecastAdjuster(Dictionary<string, double> Factors) : IRefreshForecastStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(RefreshForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRefreshForecastAdjuster(List<double> Bounds, List<double> Rates) : IRefreshForecastStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshForecast item, double valueValue)
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

public sealed record CapRefreshForecastAdjuster(double Cap) : IRefreshForecastStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRefreshForecastAdjuster(double Step) : IRefreshForecastStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRefreshForecastAdjuster(double Minimum) : IRefreshForecastStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the refresh forecast steps in order and keeps a trace of each.</summary>
public sealed class RefreshForecastEngine(List<IRefreshForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshForecastEngine DefaultRefreshForecastEngine()
    {
        return new RefreshForecastEngine(
            new List<IRefreshForecastStep>
            {
                new ChannelRefreshForecastAdjuster(new Dictionary<string, double> { ["online"] = 1.27 }),
                new TieredRefreshForecastAdjuster(
                    new List<double> { 34.0, 2827.0, 3450.0, 9399.0 },
                    new List<double> { 0.8, 1.03, 1.15, 1.04 }),
                new CapRefreshForecastAdjuster(557.0),
                new RoundingRefreshForecastAdjuster(0.25),
                new MinimumRefreshForecastAdjuster(34.1),
            },
            new List<string>(),
            1.2,
            80637.1);
    }

    public double Compute(RefreshForecast item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
