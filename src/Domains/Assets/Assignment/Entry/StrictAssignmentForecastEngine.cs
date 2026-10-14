namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict assignment forecast calculation.</summary>
public interface IStrictAssignmentForecastStep
{
    string Describe();
    double Adjust(StrictAssignmentForecast item, double valueValue);
}

public sealed record RoundingStrictAssignmentForecastAdjuster(double Step) : IStrictAssignmentForecastStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictAssignmentForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumStrictAssignmentForecastAdjuster(double Minimum) : IStrictAssignmentForecastStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictAssignmentForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameStrictAssignmentForecastAdjuster(Dictionary<string, double> Factors) : IStrictAssignmentForecastStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(StrictAssignmentForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record StrictAssignmentForecastUpdatedAtOffsetAdjuster(double Weight) : IStrictAssignmentForecastStep
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(StrictAssignmentForecast item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

/// <summary>Runs the strict assignment forecast steps in order and keeps a trace of each.</summary>
public sealed class StrictAssignmentForecastEngine(List<IStrictAssignmentForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictAssignmentForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictAssignmentForecastEngine DefaultStrictAssignmentForecastEngine()
    {
        return new StrictAssignmentForecastEngine(
            new List<IStrictAssignmentForecastStep>
            {
                new RoundingStrictAssignmentForecastAdjuster(5.0),
                new MinimumStrictAssignmentForecastAdjuster(33.9),
                new NameStrictAssignmentForecastAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.3, ["south"] = 1.32 }),
                new StrictAssignmentForecastUpdatedAtOffsetAdjuster(1.22),
            },
            new List<string>(),
            1.1,
            78360.3);
    }

    public double Run(StrictAssignmentForecast item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<StrictAssignmentForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
