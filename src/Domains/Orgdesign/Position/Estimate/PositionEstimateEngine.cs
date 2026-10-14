namespace Acme.Workforce.Orgdesign.Position.Estimate;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the position estimate calculation.</summary>
public interface IPositionEstimateStep
{
    string Describe();
    double Adjust(PositionEstimate item, double valueValue);
}

public sealed record FlaggedPositionEstimateAdjuster(Registry Flags, string Flag, double Percent) : IPositionEstimateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPositionEstimateAdjuster(double Minimum) : IPositionEstimateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PositionEstimateCapacityOffsetAdjuster(double Weight) : IPositionEstimateStep
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(PositionEstimate item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record CapPositionEstimateAdjuster(double Cap) : IPositionEstimateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPositionEstimateAdjuster(List<double> Bounds, List<double> Rates) : IPositionEstimateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionEstimate item, double valueValue)
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

public sealed record NamePositionEstimateAdjuster(Dictionary<string, double> Factors) : IPositionEstimateStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PositionEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the position estimate steps in order and keeps a trace of each.</summary>
public sealed class PositionEstimateEngine(List<IPositionEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionEstimateEngine DefaultPositionEstimateEngine(Registry flags)
    {
        return new PositionEstimateEngine(
            new List<IPositionEstimateStep>
            {
                new FlaggedPositionEstimateAdjuster(flags, "position-estimate-scheduled", -6.4),
                new MinimumPositionEstimateAdjuster(35.9),
                new PositionEstimateCapacityOffsetAdjuster(0.87),
                new CapPositionEstimateAdjuster(26525.0),
                new TieredPositionEstimateAdjuster(
                    new List<double> { 2051.0, 3317.0, 6519.0, 9262.0 },
                    new List<double> { 0.88, 0.81, 0.94, 1.12 }),
                new NamePositionEstimateAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.15, ["south"] = 0.85, ["east"] = 1.33 }),
            },
            new List<string>(),
            3.2,
            32152.3);
    }

    public double Price(PositionEstimate item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PositionEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
