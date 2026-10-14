namespace Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>One step of the succession plan threshold calculation.</summary>
public interface ISuccessionPlanThresholdStep
{
    string Describe();
    double Adjust(SuccessionPlanThreshold item, double valueValue);
}

public sealed record TieredSuccessionPlanThresholdAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanThresholdStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanThreshold item, double valueValue)
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

public sealed record SuccessionPlanThresholdCeilingOffsetAdjuster(double Weight) : ISuccessionPlanThresholdStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(SuccessionPlanThreshold item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record LocaleSuccessionPlanThresholdAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanThresholdStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(SuccessionPlanThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentSuccessionPlanThresholdAdjuster(double Percent) : ISuccessionPlanThresholdStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSuccessionPlanThresholdAdjuster(double Cap) : ISuccessionPlanThresholdStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the succession plan threshold steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanThresholdEngine(List<ISuccessionPlanThresholdStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanThresholdStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanThresholdEngine DefaultSuccessionPlanThresholdEngine()
    {
        return new SuccessionPlanThresholdEngine(
            new List<ISuccessionPlanThresholdStep>
            {
                new TieredSuccessionPlanThresholdAdjuster(
                    new List<double> { 328.0, 446.0, 3679.0 },
                    new List<double> { 0.9, 1.08, 1.2 }),
                new SuccessionPlanThresholdCeilingOffsetAdjuster(1.05),
                new LocaleSuccessionPlanThresholdAdjuster(new Dictionary<string, double> { ["retail"] = 1.37 }),
                new PercentSuccessionPlanThresholdAdjuster(-18.3),
                new CapSuccessionPlanThresholdAdjuster(3334.0),
            },
            new List<string>(),
            0.4,
            8724.1);
    }

    public double Price(SuccessionPlanThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
