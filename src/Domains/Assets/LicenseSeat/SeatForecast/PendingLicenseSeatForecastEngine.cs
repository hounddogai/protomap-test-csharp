namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending license seat forecast calculation.</summary>
public interface IPendingLicenseSeatForecastStep
{
    string Describe();
    double Adjust(PendingLicenseSeatForecast item, double valueValue);
}

public sealed record TitlePendingLicenseSeatForecastAdjuster(Dictionary<string, double> Factors) : IPendingLicenseSeatForecastStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PendingLicenseSeatForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPendingLicenseSeatForecastAdjuster(double Cap) : IPendingLicenseSeatForecastStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingLicenseSeatForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PendingLicenseSeatForecastTenureMonthsOffsetAdjuster(double Weight) : IPendingLicenseSeatForecastStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(PendingLicenseSeatForecast item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record TieredPendingLicenseSeatForecastAdjuster(List<double> Bounds, List<double> Rates) : IPendingLicenseSeatForecastStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingLicenseSeatForecast item, double valueValue)
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

/// <summary>Runs the pending license seat forecast steps in order and keeps a trace of each.</summary>
public sealed class PendingLicenseSeatForecastEngine(List<IPendingLicenseSeatForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingLicenseSeatForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingLicenseSeatForecastEngine DefaultPendingLicenseSeatForecastEngine()
    {
        return new PendingLicenseSeatForecastEngine(
            new List<IPendingLicenseSeatForecastStep>
            {
                new TitlePendingLicenseSeatForecastAdjuster(new Dictionary<string, double> { ["east"] = 0.98 }),
                new CapPendingLicenseSeatForecastAdjuster(15165.0),
                new PendingLicenseSeatForecastTenureMonthsOffsetAdjuster(0.88),
                new TieredPendingLicenseSeatForecastAdjuster(
                    new List<double> { 2095.0, 9053.0 },
                    new List<double> { 1.16, 0.92 }),
            },
            new List<string>(),
            1.2,
            58268.1);
    }

    public double Run(PendingLicenseSeatForecast item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PendingLicenseSeatForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
