namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback return plan assignment calculation.</summary>
public interface IFallbackReturnPlanAssignmentModifier
{
    string Describe();
    double Adjust(FallbackReturnPlanAssignment item, double valueValue);
}

public sealed record PercentFallbackReturnPlanAssignmentAdjuster(double Percent) : IFallbackReturnPlanAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackReturnPlanAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFallbackReturnPlanAssignmentAdjuster(double Cap) : IFallbackReturnPlanAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackReturnPlanAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedFallbackReturnPlanAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IFallbackReturnPlanAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackReturnPlanAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelFallbackReturnPlanAssignmentAdjuster(Dictionary<string, double> Factors) : IFallbackReturnPlanAssignmentModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(FallbackReturnPlanAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredFallbackReturnPlanAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IFallbackReturnPlanAssignmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackReturnPlanAssignment item, double valueValue)
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

/// <summary>Runs the fallback return plan assignment steps in order and keeps a trace of each.</summary>
public sealed class FallbackReturnPlanAssignmentEngine(List<IFallbackReturnPlanAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackReturnPlanAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackReturnPlanAssignmentEngine DefaultFallbackReturnPlanAssignmentEngine(Registry flags)
    {
        return new FallbackReturnPlanAssignmentEngine(
            new List<IFallbackReturnPlanAssignmentModifier>
            {
                new PercentFallbackReturnPlanAssignmentAdjuster(-4.8),
                new CapFallbackReturnPlanAssignmentAdjuster(42060.0),
                new FlaggedFallbackReturnPlanAssignmentAdjuster(
                    flags,
                    "fallback-return-plan-assignment-batched",
                    -1.4),
                new LabelFallbackReturnPlanAssignmentAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.91, ["north"] = 0.81, ["south"] = 1.31 }),
                new TieredFallbackReturnPlanAssignmentAdjuster(
                    new List<double> { 1021.0, 1409.0, 2571.0, 9429.0 },
                    new List<double> { 1.11, 0.9, 0.82, 0.98 }),
            },
            new List<string>(),
            2.3,
            42745.8);
    }

    public double Compute(FallbackReturnPlanAssignment item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackReturnPlanAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
