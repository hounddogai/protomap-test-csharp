namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted occupancy forecast calculation.</summary>
public interface IWeightedOccupancyForecastStep
{
    string Describe();
    double Adjust(WeightedOccupancyForecast item, double valueValue);
}

public sealed record FlaggedWeightedOccupancyForecastAdjuster(Registry Flags, string Flag, double Percent) : IWeightedOccupancyForecastStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedOccupancyForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameWeightedOccupancyForecastAdjuster(Dictionary<string, double> Factors) : IWeightedOccupancyForecastStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(WeightedOccupancyForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapWeightedOccupancyForecastAdjuster(double Cap) : IWeightedOccupancyForecastStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedOccupancyForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the weighted occupancy forecast steps in order and keeps a trace of each.</summary>
public sealed class WeightedOccupancyForecastEngine(List<IWeightedOccupancyForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedOccupancyForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedOccupancyForecastEngine DefaultWeightedOccupancyForecastEngine(Registry flags)
    {
        return new WeightedOccupancyForecastEngine(
            new List<IWeightedOccupancyForecastStep>
            {
                new FlaggedWeightedOccupancyForecastAdjuster(flags, "weighted-occupancy-forecast-seasonal", -2.3),
                new NameWeightedOccupancyForecastAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.91, ["south"] = 1.09, ["central"] = 0.78 }),
                new CapWeightedOccupancyForecastAdjuster(41635.0),
            },
            new List<string>(),
            1.1,
            78314.4);
    }

    public double Compute(WeightedOccupancyForecast item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedOccupancyForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
