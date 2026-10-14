namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling floor plan bundle calculation.</summary>
public interface IRollingFloorPlanBundleModifier
{
    string Describe();
    double Adjust(RollingFloorPlanBundle item, double valueValue);
}

public sealed record MinimumRollingFloorPlanBundleAdjuster(double Minimum) : IRollingFloorPlanBundleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingFloorPlanBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingRollingFloorPlanBundleAdjuster(double Step) : IRollingFloorPlanBundleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingFloorPlanBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRollingFloorPlanBundleAdjuster(double Percent) : IRollingFloorPlanBundleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingFloorPlanBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RollingFloorPlanBundleFteOffsetAdjuster(double Weight) : IRollingFloorPlanBundleModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(RollingFloorPlanBundle item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

/// <summary>Runs the rolling floor plan bundle steps in order and keeps a trace of each.</summary>
public sealed class RollingFloorPlanBundleEngine(List<IRollingFloorPlanBundleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingFloorPlanBundleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingFloorPlanBundleEngine DefaultRollingFloorPlanBundleEngine()
    {
        return new RollingFloorPlanBundleEngine(
            new List<IRollingFloorPlanBundleModifier>
            {
                new MinimumRollingFloorPlanBundleAdjuster(36.7),
                new RoundingRollingFloorPlanBundleAdjuster(0.05),
                new PercentRollingFloorPlanBundleAdjuster(-15.4),
                new RollingFloorPlanBundleFteOffsetAdjuster(0.96),
            },
            new List<string>(),
            3.9,
            82161.8);
    }

    public double Run(RollingFloorPlanBundle item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RollingFloorPlanBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
