namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

/// <summary>One step of the manual headcount tier calculation.</summary>
public interface IManualHeadcountTierModifier
{
    string Describe();
    double Adjust(ManualHeadcountTier item, double valueValue);
}

public sealed record TieredManualHeadcountTierAdjuster(List<double> Bounds, List<double> Rates) : IManualHeadcountTierModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualHeadcountTier item, double valueValue)
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

public sealed record LabelManualHeadcountTierAdjuster(Dictionary<string, double> Factors) : IManualHeadcountTierModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ManualHeadcountTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumManualHeadcountTierAdjuster(double Minimum) : IManualHeadcountTierModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualHeadcountTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentManualHeadcountTierAdjuster(double Percent) : IManualHeadcountTierModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualHeadcountTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualHeadcountTierAdjuster(double Cap) : IManualHeadcountTierModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualHeadcountTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the manual headcount tier steps in order and keeps a trace of each.</summary>
public sealed class ManualHeadcountTierEngine(List<IManualHeadcountTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualHeadcountTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualHeadcountTierEngine DefaultManualHeadcountTierEngine()
    {
        return new ManualHeadcountTierEngine(
            new List<IManualHeadcountTierModifier>
            {
                new TieredManualHeadcountTierAdjuster(
                    new List<double> { 5780.0, 7813.0 },
                    new List<double> { 0.89, 1.06 }),
                new LabelManualHeadcountTierAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.77, ["north"] = 1.1 }),
                new MinimumManualHeadcountTierAdjuster(15.8),
                new PercentManualHeadcountTierAdjuster(12.4),
                new CapManualHeadcountTierAdjuster(46542.0),
            },
            new List<string>(),
            3.6,
            94111.1);
    }

    public double Run(ManualHeadcountTier item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualHeadcountTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
