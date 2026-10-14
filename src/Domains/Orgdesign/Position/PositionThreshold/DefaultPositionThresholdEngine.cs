namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default position threshold calculation.</summary>
public interface IDefaultPositionThresholdFactor
{
    string Describe();
    double Adjust(DefaultPositionThreshold item, double valueValue);
}

public sealed record PercentDefaultPositionThresholdAdjuster(double Percent) : IDefaultPositionThresholdFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPositionThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultPositionThresholdAdjuster(List<double> Bounds, List<double> Rates) : IDefaultPositionThresholdFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultPositionThreshold item, double valueValue)
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

public sealed record FlaggedDefaultPositionThresholdAdjuster(Registry Flags, string Flag, double Percent) : IDefaultPositionThresholdFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultPositionThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultPositionThresholdAdjuster(double Minimum) : IDefaultPositionThresholdFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPositionThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapDefaultPositionThresholdAdjuster(double Cap) : IDefaultPositionThresholdFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultPositionThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleDefaultPositionThresholdAdjuster(Dictionary<string, double> Factors) : IDefaultPositionThresholdFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DefaultPositionThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the default position threshold steps in order and keeps a trace of each.</summary>
public sealed class DefaultPositionThresholdEngine(List<IDefaultPositionThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPositionThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPositionThresholdEngine DefaultDefaultPositionThresholdEngine(Registry flags)
    {
        return new DefaultPositionThresholdEngine(
            new List<IDefaultPositionThresholdFactor>
            {
                new PercentDefaultPositionThresholdAdjuster(20.4),
                new TieredDefaultPositionThresholdAdjuster(
                    new List<double> { 6665.0, 6827.0, 8771.0 },
                    new List<double> { 0.99, 1.11, 1.13 }),
                new FlaggedDefaultPositionThresholdAdjuster(flags, "default-position-threshold-batched", -4.0),
                new MinimumDefaultPositionThresholdAdjuster(43.3),
                new CapDefaultPositionThresholdAdjuster(20882.0),
                new TitleDefaultPositionThresholdAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.76, ["south"] = 1.33 }),
            },
            new List<string>(),
            4.1,
            47924.9);
    }

    public double Compute(DefaultPositionThreshold item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPositionThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
