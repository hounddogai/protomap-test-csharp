namespace Acme.Workforce.Orgdesign.Position.PositionAudit;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted position forecast calculation.</summary>
public interface IWeightedPositionForecastModifier
{
    string Describe();
    double Adjust(WeightedPositionForecast item, double valueValue);
}

public sealed record SourceWeightedPositionForecastAdjuster(Dictionary<string, double> Factors) : IWeightedPositionForecastModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(WeightedPositionForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedWeightedPositionForecastAdjuster(Registry Flags, string Flag, double Percent) : IWeightedPositionForecastModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedPositionForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWeightedPositionForecastAdjuster(double Cap) : IWeightedPositionForecastModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedPositionForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentWeightedPositionForecastAdjuster(double Percent) : IWeightedPositionForecastModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedPositionForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumWeightedPositionForecastAdjuster(double Minimum) : IWeightedPositionForecastModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedPositionForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredWeightedPositionForecastAdjuster(List<double> Bounds, List<double> Rates) : IWeightedPositionForecastModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedPositionForecast item, double valueValue)
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

/// <summary>Runs the weighted position forecast steps in order and keeps a trace of each.</summary>
public sealed class WeightedPositionForecastEngine(List<IWeightedPositionForecastModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedPositionForecastModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedPositionForecastEngine DefaultWeightedPositionForecastEngine(Registry flags)
    {
        return new WeightedPositionForecastEngine(
            new List<IWeightedPositionForecastModifier>
            {
                new SourceWeightedPositionForecastAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.32, ["retail"] = 0.94, ["south"] = 0.71 }),
                new FlaggedWeightedPositionForecastAdjuster(flags, "weighted-position-forecast-scheduled", 1.2),
                new CapWeightedPositionForecastAdjuster(16538.0),
                new PercentWeightedPositionForecastAdjuster(-3.1),
                new MinimumWeightedPositionForecastAdjuster(5.3),
                new TieredWeightedPositionForecastAdjuster(
                    new List<double> { 3320.0, 6186.0, 7085.0, 9363.0 },
                    new List<double> { 1.04, 0.87, 0.93, 0.98 }),
            },
            new List<string>(),
            4.8,
            78923.4);
    }

    public double Evaluate(WeightedPositionForecast item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedPositionForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
