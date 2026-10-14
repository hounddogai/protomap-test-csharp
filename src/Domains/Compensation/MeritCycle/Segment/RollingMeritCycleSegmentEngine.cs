namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling merit cycle segment calculation.</summary>
public interface IRollingMeritCycleSegmentStep
{
    string Describe();
    double Adjust(RollingMeritCycleSegment item, double valueValue);
}

public sealed record FlaggedRollingMeritCycleSegmentAdjuster(Registry Flags, string Flag, double Percent) : IRollingMeritCycleSegmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingMeritCycleSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRollingMeritCycleSegmentAdjuster(List<double> Bounds, List<double> Rates) : IRollingMeritCycleSegmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingMeritCycleSegment item, double valueValue)
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

public sealed record RollingMeritCycleSegmentBatchSizeOffsetAdjuster(double Weight) : IRollingMeritCycleSegmentStep
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(RollingMeritCycleSegment item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record MinimumRollingMeritCycleSegmentAdjuster(double Minimum) : IRollingMeritCycleSegmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingMeritCycleSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRollingMeritCycleSegmentAdjuster(double Cap) : IRollingMeritCycleSegmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingMeritCycleSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRollingMeritCycleSegmentAdjuster(double Step) : IRollingMeritCycleSegmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingMeritCycleSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the rolling merit cycle segment steps in order and keeps a trace of each.</summary>
public sealed class RollingMeritCycleSegmentEngine(List<IRollingMeritCycleSegmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingMeritCycleSegmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingMeritCycleSegmentEngine DefaultRollingMeritCycleSegmentEngine(Registry flags)
    {
        return new RollingMeritCycleSegmentEngine(
            new List<IRollingMeritCycleSegmentStep>
            {
                new FlaggedRollingMeritCycleSegmentAdjuster(flags, "rolling-merit-cycle-segment-pending", -8.2),
                new TieredRollingMeritCycleSegmentAdjuster(
                    new List<double> { 431.0, 7974.0 },
                    new List<double> { 0.96, 1.19 }),
                new RollingMeritCycleSegmentBatchSizeOffsetAdjuster(1.34),
                new MinimumRollingMeritCycleSegmentAdjuster(36.6),
                new CapRollingMeritCycleSegmentAdjuster(20741.0),
                new RoundingRollingMeritCycleSegmentAdjuster(5.0),
            },
            new List<string>(),
            1.1,
            60108.6);
    }

    public double Run(RollingMeritCycleSegment item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RollingMeritCycleSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
