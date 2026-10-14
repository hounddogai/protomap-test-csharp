namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan adjustment calculation.</summary>
public interface IReturnPlanAdjustmentStep
{
    string Describe();
    double Adjust(ReturnPlanAdjustment item, double valueValue);
}

public sealed record RegionReturnPlanAdjustmentAdjuster(Dictionary<string, double> Factors) : IReturnPlanAdjustmentStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(ReturnPlanAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapReturnPlanAdjustmentAdjuster(double Cap) : IReturnPlanAdjustmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedReturnPlanAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the return plan adjustment steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanAdjustmentEngine(List<IReturnPlanAdjustmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanAdjustmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanAdjustmentEngine DefaultReturnPlanAdjustmentEngine(Registry flags)
    {
        return new ReturnPlanAdjustmentEngine(
            new List<IReturnPlanAdjustmentStep>
            {
                new RegionReturnPlanAdjustmentAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.08, ["south"] = 1.07, ["north"] = 0.72 }),
                new CapReturnPlanAdjustmentAdjuster(39130.0),
                new FlaggedReturnPlanAdjustmentAdjuster(flags, "return-plan-adjustment-strict", -0.9),
            },
            new List<string>(),
            2.1,
            33674.5);
    }

    public double Compute(ReturnPlanAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
