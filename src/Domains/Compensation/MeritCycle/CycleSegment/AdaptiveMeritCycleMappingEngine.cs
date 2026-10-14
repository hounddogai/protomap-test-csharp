namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>One step of the adaptive merit cycle mapping calculation.</summary>
public interface IAdaptiveMeritCycleMappingStep
{
    string Describe();
    double Adjust(AdaptiveMeritCycleMapping item, double valueValue);
}

public sealed record PercentAdaptiveMeritCycleMappingAdjuster(double Percent) : IAdaptiveMeritCycleMappingStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveMeritCycleMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameAdaptiveMeritCycleMappingAdjuster(Dictionary<string, double> Factors) : IAdaptiveMeritCycleMappingStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AdaptiveMeritCycleMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAdaptiveMeritCycleMappingAdjuster(double Minimum) : IAdaptiveMeritCycleMappingStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveMeritCycleMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive merit cycle mapping steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveMeritCycleMappingEngine(List<IAdaptiveMeritCycleMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveMeritCycleMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveMeritCycleMappingEngine DefaultAdaptiveMeritCycleMappingEngine()
    {
        return new AdaptiveMeritCycleMappingEngine(
            new List<IAdaptiveMeritCycleMappingStep>
            {
                new PercentAdaptiveMeritCycleMappingAdjuster(8.1),
                new NameAdaptiveMeritCycleMappingAdjuster(new Dictionary<string, double> { ["retail"] = 0.95 }),
                new MinimumAdaptiveMeritCycleMappingAdjuster(44.1),
            },
            new List<string>(),
            2.4,
            54987.9);
    }

    public double Price(AdaptiveMeritCycleMapping item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveMeritCycleMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
