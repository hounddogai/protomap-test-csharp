namespace Acme.Workforce.Absence.Accrual.Forecast;

/// <summary>One step of the strict accrual forecast calculation.</summary>
public interface IStrictAccrualForecastFactor
{
    string Describe();
    double Adjust(StrictAccrualForecast item, double valueValue);
}

public sealed record PercentStrictAccrualForecastAdjuster(double Percent) : IStrictAccrualForecastFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictAccrualForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceStrictAccrualForecastAdjuster(Dictionary<string, double> Factors) : IStrictAccrualForecastFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(StrictAccrualForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumStrictAccrualForecastAdjuster(double Minimum) : IStrictAccrualForecastFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictAccrualForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapStrictAccrualForecastAdjuster(double Cap) : IStrictAccrualForecastFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictAccrualForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the strict accrual forecast steps in order and keeps a trace of each.</summary>
public sealed class StrictAccrualForecastEngine(List<IStrictAccrualForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictAccrualForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictAccrualForecastEngine DefaultStrictAccrualForecastEngine()
    {
        return new StrictAccrualForecastEngine(
            new List<IStrictAccrualForecastFactor>
            {
                new PercentStrictAccrualForecastAdjuster(-7.7),
                new SourceStrictAccrualForecastAdjuster(new Dictionary<string, double> { ["east"] = 1.15 }),
                new MinimumStrictAccrualForecastAdjuster(48.0),
                new CapStrictAccrualForecastAdjuster(7513.0),
            },
            new List<string>(),
            1.8,
            86400.5);
    }

    public double Price(StrictAccrualForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictAccrualForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
