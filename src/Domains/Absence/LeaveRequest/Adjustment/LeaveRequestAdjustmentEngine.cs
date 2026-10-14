namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request adjustment calculation.</summary>
public interface ILeaveRequestAdjustmentFactor
{
    string Describe();
    double Adjust(LeaveRequestAdjustment item, double valueValue);
}

public sealed record FlaggedLeaveRequestAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestAdjustmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLeaveRequestAdjustmentAdjuster(double Step) : ILeaveRequestAdjustmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLeaveRequestAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestAdjustmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestAdjustment item, double valueValue)
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

public sealed record CapLeaveRequestAdjustmentAdjuster(double Cap) : ILeaveRequestAdjustmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLeaveRequestAdjustmentAdjuster(double Minimum) : ILeaveRequestAdjustmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLeaveRequestAdjustmentAdjuster(double Percent) : ILeaveRequestAdjustmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the leave request adjustment steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestAdjustmentEngine(List<ILeaveRequestAdjustmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestAdjustmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestAdjustmentEngine DefaultLeaveRequestAdjustmentEngine(Registry flags)
    {
        return new LeaveRequestAdjustmentEngine(
            new List<ILeaveRequestAdjustmentFactor>
            {
                new FlaggedLeaveRequestAdjustmentAdjuster(flags, "leave-request-adjustment-pending", -2.5),
                new RoundingLeaveRequestAdjustmentAdjuster(0.5),
                new TieredLeaveRequestAdjustmentAdjuster(
                    new List<double> { 407.0, 910.0, 3018.0 },
                    new List<double> { 0.96, 0.96, 1.03 }),
                new CapLeaveRequestAdjustmentAdjuster(49045.0),
                new MinimumLeaveRequestAdjustmentAdjuster(40.4),
                new PercentLeaveRequestAdjustmentAdjuster(-4.0),
            },
            new List<string>(),
            2.2,
            85465.3);
    }

    public double Compute(LeaveRequestAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
