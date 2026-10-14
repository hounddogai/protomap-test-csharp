namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

/// <summary>One step of the equity grant threshold calculation.</summary>
public interface IEquityGrantThresholdModifier
{
    string Describe();
    double Adjust(EquityGrantThreshold item, double valueValue);
}

public sealed record MinimumEquityGrantThresholdAdjuster(double Minimum) : IEquityGrantThresholdModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EquityGrantThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameEquityGrantThresholdAdjuster(Dictionary<string, double> Factors) : IEquityGrantThresholdModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(EquityGrantThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentEquityGrantThresholdAdjuster(double Percent) : IEquityGrantThresholdModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EquityGrantThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredEquityGrantThresholdAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantThresholdModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantThreshold item, double valueValue)
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

/// <summary>Runs the equity grant threshold steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantThresholdEngine(List<IEquityGrantThresholdModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantThresholdModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantThresholdEngine DefaultEquityGrantThresholdEngine()
    {
        return new EquityGrantThresholdEngine(
            new List<IEquityGrantThresholdModifier>
            {
                new MinimumEquityGrantThresholdAdjuster(4.9),
                new NameEquityGrantThresholdAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.78, ["south"] = 0.71 }),
                new PercentEquityGrantThresholdAdjuster(14.1),
                new TieredEquityGrantThresholdAdjuster(
                    new List<double> { 2512.0, 2936.0, 4682.0, 6412.0 },
                    new List<double> { 1.18, 0.92, 0.97, 1.01 }),
            },
            new List<string>(),
            4.3,
            53643.7);
    }

    public double Run(EquityGrantThreshold item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
