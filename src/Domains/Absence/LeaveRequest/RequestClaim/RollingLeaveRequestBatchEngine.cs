namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling leave request batch calculation.</summary>
public interface IRollingLeaveRequestBatchFactor
{
    string Describe();
    double Adjust(RollingLeaveRequestBatch item, double valueValue);
}

public sealed record RollingLeaveRequestBatchFloorValueOffsetAdjuster(double Weight) : IRollingLeaveRequestBatchFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(RollingLeaveRequestBatch item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record FlaggedRollingLeaveRequestBatchAdjuster(Registry Flags, string Flag, double Percent) : IRollingLeaveRequestBatchFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingLeaveRequestBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRollingLeaveRequestBatchAdjuster(List<double> Bounds, List<double> Rates) : IRollingLeaveRequestBatchFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingLeaveRequestBatch item, double valueValue)
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

public sealed record RoundingRollingLeaveRequestBatchAdjuster(double Step) : IRollingLeaveRequestBatchFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingLeaveRequestBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitRollingLeaveRequestBatchAdjuster(Dictionary<string, double> Factors) : IRollingLeaveRequestBatchFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(RollingLeaveRequestBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRollingLeaveRequestBatchAdjuster(double Minimum) : IRollingLeaveRequestBatchFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingLeaveRequestBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rolling leave request batch steps in order and keeps a trace of each.</summary>
public sealed class RollingLeaveRequestBatchEngine(List<IRollingLeaveRequestBatchFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingLeaveRequestBatchFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingLeaveRequestBatchEngine DefaultRollingLeaveRequestBatchEngine(Registry flags)
    {
        return new RollingLeaveRequestBatchEngine(
            new List<IRollingLeaveRequestBatchFactor>
            {
                new RollingLeaveRequestBatchFloorValueOffsetAdjuster(1.0),
                new FlaggedRollingLeaveRequestBatchAdjuster(flags, "rolling-leave-request-batch-regional", -4.7),
                new TieredRollingLeaveRequestBatchAdjuster(
                    new List<double> { 835.0, 3716.0, 4781.0, 9291.0 },
                    new List<double> { 0.88, 0.84, 0.81, 0.98 }),
                new RoundingRollingLeaveRequestBatchAdjuster(0.5),
                new UnitRollingLeaveRequestBatchAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.3, ["central"] = 1.24 }),
                new MinimumRollingLeaveRequestBatchAdjuster(47.0),
            },
            new List<string>(),
            1.5,
            33689.3);
    }

    public double Run(RollingLeaveRequestBatch item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RollingLeaveRequestBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
