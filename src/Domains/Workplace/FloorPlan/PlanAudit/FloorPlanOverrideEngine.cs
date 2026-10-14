namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan override calculation.</summary>
public interface IFloorPlanOverrideStep
{
    string Describe();
    double Adjust(FloorPlanOverride item, double valueValue);
}

public sealed record RoundingFloorPlanOverrideAdjuster(double Step) : IFloorPlanOverrideStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredFloorPlanOverrideAdjuster(List<double> Bounds, List<double> Rates) : IFloorPlanOverrideStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FloorPlanOverride item, double valueValue)
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

public sealed record PercentFloorPlanOverrideAdjuster(double Percent) : IFloorPlanOverrideStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFloorPlanOverrideAdjuster(double Minimum) : IFloorPlanOverrideStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapFloorPlanOverrideAdjuster(double Cap) : IFloorPlanOverrideStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the floor plan override steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanOverrideEngine(List<IFloorPlanOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanOverrideEngine DefaultFloorPlanOverrideEngine()
    {
        return new FloorPlanOverrideEngine(
            new List<IFloorPlanOverrideStep>
            {
                new RoundingFloorPlanOverrideAdjuster(1.0),
                new TieredFloorPlanOverrideAdjuster(
                    new List<double> { 1524.0, 1731.0, 5247.0, 7287.0 },
                    new List<double> { 1.1, 0.99, 0.81, 0.91 }),
                new PercentFloorPlanOverrideAdjuster(15.4),
                new MinimumFloorPlanOverrideAdjuster(39.6),
                new CapFloorPlanOverrideAdjuster(49243.0),
            },
            new List<string>(),
            1.0,
            14690.3);
    }

    public double Price(FloorPlanOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
