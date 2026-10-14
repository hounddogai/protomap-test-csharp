namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending floor plan target calculation.</summary>
public interface IPendingFloorPlanTargetFactor
{
    string Describe();
    double Adjust(PendingFloorPlanTarget item, double valueValue);
}

public sealed record PercentPendingFloorPlanTargetAdjuster(double Percent) : IPendingFloorPlanTargetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingFloorPlanTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPendingFloorPlanTargetAdjuster(double Step) : IPendingFloorPlanTargetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingFloorPlanTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPendingFloorPlanTargetAdjuster(Registry Flags, string Flag, double Percent) : IPendingFloorPlanTargetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingFloorPlanTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPendingFloorPlanTargetAdjuster(List<double> Bounds, List<double> Rates) : IPendingFloorPlanTargetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingFloorPlanTarget item, double valueValue)
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

public sealed record LabelPendingFloorPlanTargetAdjuster(Dictionary<string, double> Factors) : IPendingFloorPlanTargetFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PendingFloorPlanTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pending floor plan target steps in order and keeps a trace of each.</summary>
public sealed class PendingFloorPlanTargetEngine(List<IPendingFloorPlanTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingFloorPlanTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingFloorPlanTargetEngine DefaultPendingFloorPlanTargetEngine(Registry flags)
    {
        return new PendingFloorPlanTargetEngine(
            new List<IPendingFloorPlanTargetFactor>
            {
                new PercentPendingFloorPlanTargetAdjuster(24.9),
                new RoundingPendingFloorPlanTargetAdjuster(0.5),
                new FlaggedPendingFloorPlanTargetAdjuster(flags, "pending-floor-plan-target-pending", -1.8),
                new TieredPendingFloorPlanTargetAdjuster(
                    new List<double> { 620.0, 7284.0, 9742.0, 9863.0 },
                    new List<double> { 0.82, 0.9, 1.12, 1.12 }),
                new LabelPendingFloorPlanTargetAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.9, ["retail"] = 1.25 }),
            },
            new List<string>(),
            3.1,
            51647.1);
    }

    public double Compute(PendingFloorPlanTarget item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PendingFloorPlanTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
