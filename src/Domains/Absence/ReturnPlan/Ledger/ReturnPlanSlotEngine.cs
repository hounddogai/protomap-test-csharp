namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan slot calculation.</summary>
public interface IReturnPlanSlotAdjuster
{
    string Describe();
    double Adjust(ReturnPlanSlot item, double valueValue);
}

public sealed record FlaggedReturnPlanSlotAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredReturnPlanSlotAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanSlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanSlot item, double valueValue)
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

public sealed record CapReturnPlanSlotAdjuster(double Cap) : IReturnPlanSlotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the return plan slot steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanSlotEngine(List<IReturnPlanSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanSlotEngine DefaultReturnPlanSlotEngine(Registry flags)
    {
        return new ReturnPlanSlotEngine(
            new List<IReturnPlanSlotAdjuster>
            {
                new FlaggedReturnPlanSlotAdjuster(flags, "return-plan-slot-manual", 3.3),
                new TieredReturnPlanSlotAdjuster(
                    new List<double> { 6789.0, 6926.0 },
                    new List<double> { 0.95, 1.18 }),
                new CapReturnPlanSlotAdjuster(20026.0),
            },
            new List<string>(),
            1.8,
            33477.0);
    }

    public double Run(ReturnPlanSlot item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
