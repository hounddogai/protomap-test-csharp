namespace Acme.Workforce.Engagement.ActionPlan.PlanSegment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback action plan segment calculation.</summary>
public interface IFallbackActionPlanSegmentFactor
{
    string Describe();
    double Adjust(FallbackActionPlanSegment item, double valueValue);
}

public sealed record FallbackActionPlanSegmentFloorValueOffsetAdjuster(double Weight) : IFallbackActionPlanSegmentFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(FallbackActionPlanSegment item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record FlaggedFallbackActionPlanSegmentAdjuster(Registry Flags, string Flag, double Percent) : IFallbackActionPlanSegmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackActionPlanSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentFallbackActionPlanSegmentAdjuster(Dictionary<string, double> Factors) : IFallbackActionPlanSegmentFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(FallbackActionPlanSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapFallbackActionPlanSegmentAdjuster(double Cap) : IFallbackActionPlanSegmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackActionPlanSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingFallbackActionPlanSegmentAdjuster(double Step) : IFallbackActionPlanSegmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackActionPlanSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFallbackActionPlanSegmentAdjuster(double Minimum) : IFallbackActionPlanSegmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackActionPlanSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the fallback action plan segment steps in order and keeps a trace of each.</summary>
public sealed class FallbackActionPlanSegmentEngine(List<IFallbackActionPlanSegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackActionPlanSegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackActionPlanSegmentEngine DefaultFallbackActionPlanSegmentEngine(Registry flags)
    {
        return new FallbackActionPlanSegmentEngine(
            new List<IFallbackActionPlanSegmentFactor>
            {
                new FallbackActionPlanSegmentFloorValueOffsetAdjuster(0.85),
                new FlaggedFallbackActionPlanSegmentAdjuster(flags, "fallback-action-plan-segment-seasonal", -4.8),
                new SegmentFallbackActionPlanSegmentAdjuster(new Dictionary<string, double> { ["west"] = 0.82 }),
                new CapFallbackActionPlanSegmentAdjuster(13399.0),
                new RoundingFallbackActionPlanSegmentAdjuster(0.1),
                new MinimumFallbackActionPlanSegmentAdjuster(32.7),
            },
            new List<string>(),
            3.7,
            76317.6);
    }

    public double Run(FallbackActionPlanSegment item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackActionPlanSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
