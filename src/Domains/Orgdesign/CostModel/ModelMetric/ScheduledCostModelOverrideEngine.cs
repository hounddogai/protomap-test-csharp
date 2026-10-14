namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>One step of the scheduled cost model override calculation.</summary>
public interface IScheduledCostModelOverrideStep
{
    string Describe();
    double Adjust(ScheduledCostModelOverride item, double valueValue);
}

public sealed record TieredScheduledCostModelOverrideAdjuster(List<double> Bounds, List<double> Rates) : IScheduledCostModelOverrideStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledCostModelOverride item, double valueValue)
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

public sealed record ScheduledCostModelOverrideLeaveDaysOffsetAdjuster(double Weight) : IScheduledCostModelOverrideStep
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(ScheduledCostModelOverride item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record NameScheduledCostModelOverrideAdjuster(Dictionary<string, double> Factors) : IScheduledCostModelOverrideStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ScheduledCostModelOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scheduled cost model override steps in order and keeps a trace of each.</summary>
public sealed class ScheduledCostModelOverrideEngine(List<IScheduledCostModelOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledCostModelOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledCostModelOverrideEngine DefaultScheduledCostModelOverrideEngine()
    {
        return new ScheduledCostModelOverrideEngine(
            new List<IScheduledCostModelOverrideStep>
            {
                new TieredScheduledCostModelOverrideAdjuster(
                    new List<double> { 677.0, 1806.0, 8750.0 },
                    new List<double> { 0.97, 1.13, 1.14 }),
                new ScheduledCostModelOverrideLeaveDaysOffsetAdjuster(0.81),
                new NameScheduledCostModelOverrideAdjuster(new Dictionary<string, double> { ["west"] = 1.08 }),
            },
            new List<string>(),
            4.1,
            34449.5);
    }

    public double Price(ScheduledCostModelOverride item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledCostModelOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
