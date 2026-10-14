namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path forecast calculation.</summary>
public interface ICareerPathForecastModifier
{
    string Describe();
    double Adjust(CareerPathForecast item, double valueValue);
}

public sealed record RoundingCareerPathForecastAdjuster(double Step) : ICareerPathForecastModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentCareerPathForecastAdjuster(double Percent) : ICareerPathForecastModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CareerPathForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCareerPathForecastAdjuster(double Minimum) : ICareerPathForecastModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the career path forecast steps in order and keeps a trace of each.</summary>
public sealed class CareerPathForecastEngine(List<ICareerPathForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathForecastEngine DefaultCareerPathForecastEngine()
    {
        return new CareerPathForecastEngine(
            new List<ICareerPathForecastModifier>
            {
                new RoundingCareerPathForecastAdjuster(0.25),
                new PercentCareerPathForecastAdjuster(7.4),
                new MinimumCareerPathForecastAdjuster(7.2),
            },
            new List<string>(),
            4.3,
            51243.2);
    }

    public double Price(CareerPathForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
