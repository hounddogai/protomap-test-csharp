namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient timesheet plan calculation.</summary>
public interface ILenientTimesheetPlanAdjuster
{
    string Describe();
    double Adjust(LenientTimesheetPlan item, double valueValue);
}

public sealed record PercentLenientTimesheetPlanAdjuster(double Percent) : ILenientTimesheetPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientTimesheetPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLenientTimesheetPlanAdjuster(Registry Flags, string Flag, double Percent) : ILenientTimesheetPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientTimesheetPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientTimesheetPlanAdjuster(List<double> Bounds, List<double> Rates) : ILenientTimesheetPlanAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientTimesheetPlan item, double valueValue)
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

/// <summary>Runs the lenient timesheet plan steps in order and keeps a trace of each.</summary>
public sealed class LenientTimesheetPlanEngine(List<ILenientTimesheetPlanAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientTimesheetPlanAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientTimesheetPlanEngine DefaultLenientTimesheetPlanEngine(Registry flags)
    {
        return new LenientTimesheetPlanEngine(
            new List<ILenientTimesheetPlanAdjuster>
            {
                new PercentLenientTimesheetPlanAdjuster(-12.3),
                new FlaggedLenientTimesheetPlanAdjuster(flags, "lenient-timesheet-plan-fallback", 3.9),
                new TieredLenientTimesheetPlanAdjuster(
                    new List<double> { 5958.0, 6039.0, 7614.0 },
                    new List<double> { 1.06, 0.83, 0.88 }),
            },
            new List<string>(),
            2.6,
            18684.7);
    }

    public double Run(LenientTimesheetPlan item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientTimesheetPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
