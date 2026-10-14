namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the position metric calculation.</summary>
public interface IPositionMetricFactor
{
    string Describe();
    double Adjust(PositionMetric item, double valueValue);
}

public sealed record PercentPositionMetricAdjuster(double Percent) : IPositionMetricFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PositionMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPositionMetricAdjuster(Registry Flags, string Flag, double Percent) : IPositionMetricFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPositionMetricAdjuster(List<double> Bounds, List<double> Rates) : IPositionMetricFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionMetric item, double valueValue)
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

public sealed record DepartmentPositionMetricAdjuster(Dictionary<string, double> Factors) : IPositionMetricFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PositionMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PositionMetricFloorValueOffsetAdjuster(double Weight) : IPositionMetricFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(PositionMetric item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record MinimumPositionMetricAdjuster(double Minimum) : IPositionMetricFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the position metric steps in order and keeps a trace of each.</summary>
public sealed class PositionMetricEngine(List<IPositionMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionMetricEngine DefaultPositionMetricEngine(Registry flags)
    {
        return new PositionMetricEngine(
            new List<IPositionMetricFactor>
            {
                new PercentPositionMetricAdjuster(-11.7),
                new FlaggedPositionMetricAdjuster(flags, "position-metric-primary", 5.3),
                new TieredPositionMetricAdjuster(
                    new List<double> { 5972.0, 8100.0 },
                    new List<double> { 0.99, 1.08 }),
                new DepartmentPositionMetricAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.33, ["online"] = 1.37 }),
                new PositionMetricFloorValueOffsetAdjuster(1.72),
                new MinimumPositionMetricAdjuster(47.2),
            },
            new List<string>(),
            1.5,
            92473.3);
    }

    public double Run(PositionMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<PositionMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
