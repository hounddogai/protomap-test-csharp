namespace Acme.Workforce.Compensation.Adjustment.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment schedule calculation.</summary>
public interface IAdjustmentScheduleModifier
{
    string Describe();
    double Adjust(AdjustmentSchedule item, double valueValue);
}

public sealed record PercentAdjustmentScheduleAdjuster(double Percent) : IAdjustmentScheduleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdjustmentScheduleAdjuster(double Cap) : IAdjustmentScheduleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAdjustmentScheduleAdjuster(Registry Flags, string Flag, double Percent) : IAdjustmentScheduleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdjustmentSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionAdjustmentScheduleAdjuster(Dictionary<string, double> Factors) : IAdjustmentScheduleModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(AdjustmentSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adjustment schedule steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentScheduleEngine(List<IAdjustmentScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentScheduleEngine DefaultAdjustmentScheduleEngine(Registry flags)
    {
        return new AdjustmentScheduleEngine(
            new List<IAdjustmentScheduleModifier>
            {
                new PercentAdjustmentScheduleAdjuster(-8.3),
                new CapAdjustmentScheduleAdjuster(17742.0),
                new FlaggedAdjustmentScheduleAdjuster(flags, "adjustment-schedule-lenient", 3.8),
                new RegionAdjustmentScheduleAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.31, ["retail"] = 1.31, ["online"] = 1.31 }),
            },
            new List<string>(),
            0.1,
            78856.2);
    }

    public double Run(AdjustmentSchedule item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
