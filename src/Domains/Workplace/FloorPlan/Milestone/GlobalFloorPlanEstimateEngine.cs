namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global floor plan estimate calculation.</summary>
public interface IGlobalFloorPlanEstimateStep
{
    string Describe();
    double Adjust(GlobalFloorPlanEstimate item, double valueValue);
}

public sealed record PercentGlobalFloorPlanEstimateAdjuster(double Percent) : IGlobalFloorPlanEstimateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalFloorPlanEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedGlobalFloorPlanEstimateAdjuster(Registry Flags, string Flag, double Percent) : IGlobalFloorPlanEstimateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalFloorPlanEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record GlobalFloorPlanEstimateEndsAtOffsetAdjuster(double Weight) : IGlobalFloorPlanEstimateStep
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(GlobalFloorPlanEstimate item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record MinimumGlobalFloorPlanEstimateAdjuster(double Minimum) : IGlobalFloorPlanEstimateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalFloorPlanEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapGlobalFloorPlanEstimateAdjuster(double Cap) : IGlobalFloorPlanEstimateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalFloorPlanEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingGlobalFloorPlanEstimateAdjuster(double Step) : IGlobalFloorPlanEstimateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalFloorPlanEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global floor plan estimate steps in order and keeps a trace of each.</summary>
public sealed class GlobalFloorPlanEstimateEngine(List<IGlobalFloorPlanEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalFloorPlanEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalFloorPlanEstimateEngine DefaultGlobalFloorPlanEstimateEngine(Registry flags)
    {
        return new GlobalFloorPlanEstimateEngine(
            new List<IGlobalFloorPlanEstimateStep>
            {
                new PercentGlobalFloorPlanEstimateAdjuster(11.7),
                new FlaggedGlobalFloorPlanEstimateAdjuster(flags, "global-floor-plan-estimate-manual", -5.7),
                new GlobalFloorPlanEstimateEndsAtOffsetAdjuster(1.78),
                new MinimumGlobalFloorPlanEstimateAdjuster(40.5),
                new CapGlobalFloorPlanEstimateAdjuster(31370.0),
                new RoundingGlobalFloorPlanEstimateAdjuster(5.0),
            },
            new List<string>(),
            3.1,
            98015.6);
    }

    public double Compute(GlobalFloorPlanEstimate item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<GlobalFloorPlanEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
