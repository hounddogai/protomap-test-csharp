namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled leave request exception calculation.</summary>
public interface IScheduledLeaveRequestExceptionModifier
{
    string Describe();
    double Adjust(ScheduledLeaveRequestException item, double valueValue);
}

public sealed record CapScheduledLeaveRequestExceptionAdjuster(double Cap) : IScheduledLeaveRequestExceptionModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledLeaveRequestException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScheduledLeaveRequestExceptionAdjuster(double Minimum) : IScheduledLeaveRequestExceptionModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledLeaveRequestException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ScheduledLeaveRequestExceptionThresholdOffsetAdjuster(double Weight) : IScheduledLeaveRequestExceptionModifier
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(ScheduledLeaveRequestException item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record FlaggedScheduledLeaveRequestExceptionAdjuster(Registry Flags, string Flag, double Percent) : IScheduledLeaveRequestExceptionModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledLeaveRequestException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScheduledLeaveRequestExceptionAdjuster(List<double> Bounds, List<double> Rates) : IScheduledLeaveRequestExceptionModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledLeaveRequestException item, double valueValue)
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

/// <summary>Runs the scheduled leave request exception steps in order and keeps a trace of each.</summary>
public sealed class ScheduledLeaveRequestExceptionEngine(List<IScheduledLeaveRequestExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledLeaveRequestExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledLeaveRequestExceptionEngine DefaultScheduledLeaveRequestExceptionEngine(Registry flags)
    {
        return new ScheduledLeaveRequestExceptionEngine(
            new List<IScheduledLeaveRequestExceptionModifier>
            {
                new CapScheduledLeaveRequestExceptionAdjuster(5376.0),
                new MinimumScheduledLeaveRequestExceptionAdjuster(12.1),
                new ScheduledLeaveRequestExceptionThresholdOffsetAdjuster(0.61),
                new FlaggedScheduledLeaveRequestExceptionAdjuster(
                    flags,
                    "scheduled-leave-request-exception-capped",
                    2.0),
                new TieredScheduledLeaveRequestExceptionAdjuster(
                    new List<double> { 1436.0, 3257.0, 6792.0 },
                    new List<double> { 1.16, 1.2, 1.2 }),
            },
            new List<string>(),
            4.8,
            36863.1);
    }

    public double Compute(ScheduledLeaveRequestException item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledLeaveRequestException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
