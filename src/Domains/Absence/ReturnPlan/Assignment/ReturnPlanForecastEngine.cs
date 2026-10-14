namespace Acme.Workforce.Absence.ReturnPlan.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan forecast calculation.</summary>
public interface IReturnPlanForecastAdjuster
{
    string Describe();
    double Adjust(ReturnPlanForecast item, double valueValue);
}

public sealed record CapReturnPlanForecastAdjuster(double Cap) : IReturnPlanForecastAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumReturnPlanForecastAdjuster(double Minimum) : IReturnPlanForecastAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReturnPlanForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredReturnPlanForecastAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanForecastAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanForecast item, double valueValue)
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

public sealed record TeamReturnPlanForecastAdjuster(Dictionary<string, double> Factors) : IReturnPlanForecastAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(ReturnPlanForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentReturnPlanForecastAdjuster(double Percent) : IReturnPlanForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReturnPlanForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the return plan forecast steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanForecastEngine(List<IReturnPlanForecastAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanForecastAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanForecastEngine DefaultReturnPlanForecastEngine()
    {
        return new ReturnPlanForecastEngine(
            new List<IReturnPlanForecastAdjuster>
            {
                new CapReturnPlanForecastAdjuster(48360.0),
                new MinimumReturnPlanForecastAdjuster(35.7),
                new TieredReturnPlanForecastAdjuster(
                    new List<double> { 16.0, 5122.0, 6077.0, 6889.0 },
                    new List<double> { 0.82, 1.13, 0.87, 0.87 }),
                new TeamReturnPlanForecastAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.4, ["west"] = 0.76 }),
                new PercentReturnPlanForecastAdjuster(-22.9),
            },
            new List<string>(),
            0.7,
            87638.8);
    }

    public double Compute(ReturnPlanForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
