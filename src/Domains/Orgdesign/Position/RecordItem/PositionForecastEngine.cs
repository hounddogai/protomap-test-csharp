namespace Acme.Workforce.Orgdesign.Position.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position forecast calculation.</summary>
public interface IPositionForecastStep
{
    string Describe();
    double Adjust(PositionForecast item, double valueValue);
}

public sealed record PositionForecastWeightOffsetAdjuster(double Weight) : IPositionForecastStep
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(PositionForecast item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record LabelPositionForecastAdjuster(Dictionary<string, double> Factors) : IPositionForecastStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PositionForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPositionForecastAdjuster(double Percent) : IPositionForecastStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PositionForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPositionForecastAdjuster(double Step) : IPositionForecastStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPositionForecastAdjuster(List<double> Bounds, List<double> Rates) : IPositionForecastStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionForecast item, double valueValue)
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

public sealed record MinimumPositionForecastAdjuster(double Minimum) : IPositionForecastStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the position forecast steps in order and keeps a trace of each.</summary>
public sealed class PositionForecastEngine(List<IPositionForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionForecastEngine DefaultPositionForecastEngine()
    {
        return new PositionForecastEngine(
            new List<IPositionForecastStep>
            {
                new PositionForecastWeightOffsetAdjuster(0.06),
                new LabelPositionForecastAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.29, ["central"] = 0.94, ["west"] = 0.96 }),
                new PercentPositionForecastAdjuster(16.7),
                new RoundingPositionForecastAdjuster(0.5),
                new TieredPositionForecastAdjuster(
                    new List<double> { 985.0, 1342.0, 2289.0, 5367.0 },
                    new List<double> { 1.11, 1.03, 0.85, 1.01 }),
                new MinimumPositionForecastAdjuster(43.0),
            },
            new List<string>(),
            3.8,
            69775.9);
    }

    public double Evaluate(PositionForecast item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PositionForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
