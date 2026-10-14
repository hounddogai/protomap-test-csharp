namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient adjustment schedule calculation.</summary>
public interface ILenientAdjustmentScheduleStep
{
    string Describe();
    double Adjust(LenientAdjustmentSchedule item, double valueValue);
}

public sealed record FlaggedLenientAdjustmentScheduleAdjuster(Registry Flags, string Flag, double Percent) : ILenientAdjustmentScheduleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientAdjustmentSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLenientAdjustmentScheduleAdjuster(double Percent) : ILenientAdjustmentScheduleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientAdjustmentSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientAdjustmentScheduleAdjuster(double Step) : ILenientAdjustmentScheduleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientAdjustmentSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameLenientAdjustmentScheduleAdjuster(Dictionary<string, double> Factors) : ILenientAdjustmentScheduleStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LenientAdjustmentSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLenientAdjustmentScheduleAdjuster(double Cap) : ILenientAdjustmentScheduleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientAdjustmentSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the lenient adjustment schedule steps in order and keeps a trace of each.</summary>
public sealed class LenientAdjustmentScheduleEngine(List<ILenientAdjustmentScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientAdjustmentScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientAdjustmentScheduleEngine DefaultLenientAdjustmentScheduleEngine(Registry flags)
    {
        return new LenientAdjustmentScheduleEngine(
            new List<ILenientAdjustmentScheduleStep>
            {
                new FlaggedLenientAdjustmentScheduleAdjuster(flags, "lenient-adjustment-schedule-adaptive", -1.0),
                new PercentLenientAdjustmentScheduleAdjuster(-0.3),
                new RoundingLenientAdjustmentScheduleAdjuster(0.05),
                new NameLenientAdjustmentScheduleAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.05, ["retail"] = 1.13, ["north"] = 0.73 }),
                new CapLenientAdjustmentScheduleAdjuster(27503.0),
            },
            new List<string>(),
            3.0,
            54728.1);
    }

    public double Price(LenientAdjustmentSchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientAdjustmentSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
