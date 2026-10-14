namespace Acme.Workforce.Workplace.FloorPlan.Audit;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the fallback floor plan request calculation.</summary>
public interface IFallbackFloorPlanRequestAdjuster
{
    string Describe();
    double Adjust(FallbackFloorPlanRequest item, double valueValue);
}

public sealed record FlaggedFallbackFloorPlanRequestAdjuster(Registry Flags, string Flag, double Percent) : IFallbackFloorPlanRequestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackFloorPlanRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFallbackFloorPlanRequestAdjuster(double Minimum) : IFallbackFloorPlanRequestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackFloorPlanRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapFallbackFloorPlanRequestAdjuster(double Cap) : IFallbackFloorPlanRequestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackFloorPlanRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the fallback floor plan request steps in order and keeps a trace of each.</summary>
public sealed class FallbackFloorPlanRequestEngine(List<IFallbackFloorPlanRequestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackFloorPlanRequestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackFloorPlanRequestEngine DefaultFallbackFloorPlanRequestEngine(Registry flags)
    {
        return new FallbackFloorPlanRequestEngine(
            new List<IFallbackFloorPlanRequestAdjuster>
            {
                new FlaggedFallbackFloorPlanRequestAdjuster(flags, "fallback-floor-plan-request-capped", 1.2),
                new MinimumFallbackFloorPlanRequestAdjuster(22.4),
                new CapFallbackFloorPlanRequestAdjuster(25183.0),
            },
            new List<string>(),
            2.6,
            12014.9);
    }

    public double Price(FallbackFloorPlanRequest item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackFloorPlanRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
