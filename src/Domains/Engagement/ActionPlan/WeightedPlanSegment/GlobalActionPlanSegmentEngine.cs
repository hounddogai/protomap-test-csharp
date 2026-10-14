namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global action plan segment calculation.</summary>
public interface IGlobalActionPlanSegmentAdjuster
{
    string Describe();
    double Adjust(GlobalActionPlanSegment item, double valueValue);
}

public sealed record FlaggedGlobalActionPlanSegmentAdjuster(Registry Flags, string Flag, double Percent) : IGlobalActionPlanSegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalActionPlanSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredGlobalActionPlanSegmentAdjuster(List<double> Bounds, List<double> Rates) : IGlobalActionPlanSegmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalActionPlanSegment item, double valueValue)
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

public sealed record GlobalActionPlanSegmentRatioOffsetAdjuster(double Weight) : IGlobalActionPlanSegmentAdjuster
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(GlobalActionPlanSegment item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record CapGlobalActionPlanSegmentAdjuster(double Cap) : IGlobalActionPlanSegmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalActionPlanSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingGlobalActionPlanSegmentAdjuster(double Step) : IGlobalActionPlanSegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalActionPlanSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global action plan segment steps in order and keeps a trace of each.</summary>
public sealed class GlobalActionPlanSegmentEngine(List<IGlobalActionPlanSegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalActionPlanSegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalActionPlanSegmentEngine DefaultGlobalActionPlanSegmentEngine(Registry flags)
    {
        return new GlobalActionPlanSegmentEngine(
            new List<IGlobalActionPlanSegmentAdjuster>
            {
                new FlaggedGlobalActionPlanSegmentAdjuster(flags, "global-action-plan-segment-default", 7.4),
                new TieredGlobalActionPlanSegmentAdjuster(
                    new List<double> { 1177.0, 3239.0, 5443.0, 7466.0 },
                    new List<double> { 1.0, 0.9, 1.16, 0.99 }),
                new GlobalActionPlanSegmentRatioOffsetAdjuster(0.35),
                new CapGlobalActionPlanSegmentAdjuster(11881.0),
                new RoundingGlobalActionPlanSegmentAdjuster(5.0),
            },
            new List<string>(),
            1.9,
            36489.4);
    }

    public double Compute(GlobalActionPlanSegment item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalActionPlanSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
