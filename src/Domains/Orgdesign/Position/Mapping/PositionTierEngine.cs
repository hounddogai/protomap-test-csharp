namespace Acme.Workforce.Orgdesign.Position.Mapping;

/// <summary>One step of the position tier calculation.</summary>
public interface IPositionTierFactor
{
    string Describe();
    double Adjust(PositionTier item, double valueValue);
}

public sealed record TitlePositionTierAdjuster(Dictionary<string, double> Factors) : IPositionTierFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PositionTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPositionTierAdjuster(double Cap) : IPositionTierFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPositionTierAdjuster(List<double> Bounds, List<double> Rates) : IPositionTierFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionTier item, double valueValue)
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

public sealed record PercentPositionTierAdjuster(double Percent) : IPositionTierFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PositionTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPositionTierAdjuster(double Minimum) : IPositionTierFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the position tier steps in order and keeps a trace of each.</summary>
public sealed class PositionTierEngine(List<IPositionTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionTierEngine DefaultPositionTierEngine()
    {
        return new PositionTierEngine(
            new List<IPositionTierFactor>
            {
                new TitlePositionTierAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.74, ["retail"] = 1.32, ["west"] = 1.15 }),
                new CapPositionTierAdjuster(4207.0),
                new TieredPositionTierAdjuster(
                    new List<double> { 665.0, 3310.0, 7097.0 },
                    new List<double> { 1.0, 0.92, 1.07 }),
                new PercentPositionTierAdjuster(22.5),
                new MinimumPositionTierAdjuster(13.8),
            },
            new List<string>(),
            1.7,
            39097.6);
    }

    public double Run(PositionTier item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PositionTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
