namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered leave request plan calculation.</summary>
public interface ITieredLeaveRequestPlanFactor
{
    string Describe();
    double Adjust(TieredLeaveRequestPlan item, double valueValue);
}

public sealed record FlaggedTieredLeaveRequestPlanAdjuster(Registry Flags, string Flag, double Percent) : ITieredLeaveRequestPlanFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredLeaveRequestPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLeaveRequestPlanAttemptsOffsetAdjuster(double Weight) : ITieredLeaveRequestPlanFactor
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(TieredLeaveRequestPlan item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record CapTieredLeaveRequestPlanAdjuster(double Cap) : ITieredLeaveRequestPlanFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredLeaveRequestPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingTieredLeaveRequestPlanAdjuster(double Step) : ITieredLeaveRequestPlanFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredLeaveRequestPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTieredLeaveRequestPlanAdjuster(double Minimum) : ITieredLeaveRequestPlanFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredLeaveRequestPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelTieredLeaveRequestPlanAdjuster(Dictionary<string, double> Factors) : ITieredLeaveRequestPlanFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TieredLeaveRequestPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the tiered leave request plan steps in order and keeps a trace of each.</summary>
public sealed class TieredLeaveRequestPlanEngine(List<ITieredLeaveRequestPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredLeaveRequestPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredLeaveRequestPlanEngine DefaultTieredLeaveRequestPlanEngine(Registry flags)
    {
        return new TieredLeaveRequestPlanEngine(
            new List<ITieredLeaveRequestPlanFactor>
            {
                new FlaggedTieredLeaveRequestPlanAdjuster(flags, "tiered-leave-request-plan-rolling", 5.4),
                new TieredLeaveRequestPlanAttemptsOffsetAdjuster(1.63),
                new CapTieredLeaveRequestPlanAdjuster(39169.0),
                new RoundingTieredLeaveRequestPlanAdjuster(0.25),
                new MinimumTieredLeaveRequestPlanAdjuster(27.9),
                new LabelTieredLeaveRequestPlanAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.36, ["west"] = 1.06, ["online"] = 0.86 }),
            },
            new List<string>(),
            0.6,
            54024.7);
    }

    public double Compute(TieredLeaveRequestPlan item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TieredLeaveRequestPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
