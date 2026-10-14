namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path metric calculation.</summary>
public interface ICareerPathMetricModifier
{
    string Describe();
    double Adjust(CareerPathMetric item, double valueValue);
}

public sealed record CareerPathMetricCapacityOffsetAdjuster(double Weight) : ICareerPathMetricModifier
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(CareerPathMetric item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record RoundingCareerPathMetricAdjuster(double Step) : ICareerPathMetricModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedCareerPathMetricAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathMetricModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCareerPathMetricAdjuster(double Minimum) : ICareerPathMetricModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCareerPathMetricAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathMetricModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathMetric item, double valueValue)
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

public sealed record CapCareerPathMetricAdjuster(double Cap) : ICareerPathMetricModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the career path metric steps in order and keeps a trace of each.</summary>
public sealed class CareerPathMetricEngine(List<ICareerPathMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathMetricEngine DefaultCareerPathMetricEngine(Registry flags)
    {
        return new CareerPathMetricEngine(
            new List<ICareerPathMetricModifier>
            {
                new CareerPathMetricCapacityOffsetAdjuster(1.24),
                new RoundingCareerPathMetricAdjuster(0.1),
                new FlaggedCareerPathMetricAdjuster(flags, "career-path-metric-regional", -1.7),
                new MinimumCareerPathMetricAdjuster(23.7),
                new TieredCareerPathMetricAdjuster(
                    new List<double> { 357.0, 2530.0, 9319.0, 9451.0 },
                    new List<double> { 1.07, 1.01, 0.95, 0.99 }),
                new CapCareerPathMetricAdjuster(5598.0),
            },
            new List<string>(),
            2.2,
            53035.6);
    }

    public double Run(CareerPathMetric item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
