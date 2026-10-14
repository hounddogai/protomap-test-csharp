namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>One step of the default position adjustment calculation.</summary>
public interface IDefaultPositionAdjustmentAdjuster
{
    string Describe();
    double Adjust(DefaultPositionAdjustment item, double valueValue);
}

public sealed record TieredDefaultPositionAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IDefaultPositionAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultPositionAdjustment item, double valueValue)
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

public sealed record PercentDefaultPositionAdjustmentAdjuster(double Percent) : IDefaultPositionAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPositionAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleDefaultPositionAdjustmentAdjuster(Dictionary<string, double> Factors) : IDefaultPositionAdjustmentAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DefaultPositionAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the default position adjustment steps in order and keeps a trace of each.</summary>
public sealed class DefaultPositionAdjustmentEngine(List<IDefaultPositionAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPositionAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPositionAdjustmentEngine DefaultDefaultPositionAdjustmentEngine()
    {
        return new DefaultPositionAdjustmentEngine(
            new List<IDefaultPositionAdjustmentAdjuster>
            {
                new TieredDefaultPositionAdjustmentAdjuster(
                    new List<double> { 7828.0, 9613.0 },
                    new List<double> { 0.85, 0.95 }),
                new PercentDefaultPositionAdjustmentAdjuster(-0.9),
                new TitleDefaultPositionAdjustmentAdjuster(new Dictionary<string, double> { ["west"] = 1.29 }),
            },
            new List<string>(),
            1.3,
            35298.5);
    }

    public double Run(DefaultPositionAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPositionAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
