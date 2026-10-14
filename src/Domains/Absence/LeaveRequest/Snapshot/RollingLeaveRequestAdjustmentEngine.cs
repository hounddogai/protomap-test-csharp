namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling leave request adjustment calculation.</summary>
public interface IRollingLeaveRequestAdjustmentAdjuster
{
    string Describe();
    double Adjust(RollingLeaveRequestAdjustment item, double valueValue);
}

public sealed record TieredRollingLeaveRequestAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IRollingLeaveRequestAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingLeaveRequestAdjustment item, double valueValue)
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

public sealed record FlaggedRollingLeaveRequestAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IRollingLeaveRequestAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingLeaveRequestAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRollingLeaveRequestAdjustmentAdjuster(double Minimum) : IRollingLeaveRequestAdjustmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingLeaveRequestAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRollingLeaveRequestAdjustmentAdjuster(double Cap) : IRollingLeaveRequestAdjustmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingLeaveRequestAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling leave request adjustment steps in order and keeps a trace of each.</summary>
public sealed class RollingLeaveRequestAdjustmentEngine(List<IRollingLeaveRequestAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingLeaveRequestAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingLeaveRequestAdjustmentEngine DefaultRollingLeaveRequestAdjustmentEngine(Registry flags)
    {
        return new RollingLeaveRequestAdjustmentEngine(
            new List<IRollingLeaveRequestAdjustmentAdjuster>
            {
                new TieredRollingLeaveRequestAdjustmentAdjuster(
                    new List<double> { 3163.0, 9036.0 },
                    new List<double> { 1.01, 0.84 }),
                new FlaggedRollingLeaveRequestAdjustmentAdjuster(
                    flags,
                    "rolling-leave-request-adjustment-capped",
                    -1.2),
                new MinimumRollingLeaveRequestAdjustmentAdjuster(13.7),
                new CapRollingLeaveRequestAdjustmentAdjuster(36490.0),
            },
            new List<string>(),
            4.5,
            5313.9);
    }

    public double Run(RollingLeaveRequestAdjustment item)
    {
        var current = item.ProjectedValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<RollingLeaveRequestAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
