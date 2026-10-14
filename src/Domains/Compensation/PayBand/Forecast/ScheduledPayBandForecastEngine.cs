namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled pay band forecast calculation.</summary>
public interface IScheduledPayBandForecastFactor
{
    string Describe();
    double Adjust(ScheduledPayBandForecast item, double valueValue);
}

public sealed record TieredScheduledPayBandForecastAdjuster(List<double> Bounds, List<double> Rates) : IScheduledPayBandForecastFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledPayBandForecast item, double valueValue)
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

public sealed record ScheduledPayBandForecastTenureMonthsOffsetAdjuster(double Weight) : IScheduledPayBandForecastFactor
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(ScheduledPayBandForecast item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record TeamScheduledPayBandForecastAdjuster(Dictionary<string, double> Factors) : IScheduledPayBandForecastFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(ScheduledPayBandForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scheduled pay band forecast steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPayBandForecastEngine(List<IScheduledPayBandForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPayBandForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPayBandForecastEngine DefaultScheduledPayBandForecastEngine()
    {
        return new ScheduledPayBandForecastEngine(
            new List<IScheduledPayBandForecastFactor>
            {
                new TieredScheduledPayBandForecastAdjuster(
                    new List<double> { 576.0, 3464.0, 6936.0, 8744.0 },
                    new List<double> { 0.8, 1.05, 1.19, 0.84 }),
                new ScheduledPayBandForecastTenureMonthsOffsetAdjuster(1.08),
                new TeamScheduledPayBandForecastAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.13, ["west"] = 1.0, ["retail"] = 1.2 }),
            },
            new List<string>(),
            2.0,
            64052.3);
    }

    public double Compute(ScheduledPayBandForecast item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPayBandForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
