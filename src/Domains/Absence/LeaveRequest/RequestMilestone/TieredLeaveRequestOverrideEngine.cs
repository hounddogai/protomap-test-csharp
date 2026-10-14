namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered leave request override calculation.</summary>
public interface ITieredLeaveRequestOverrideFactor
{
    string Describe();
    double Adjust(TieredLeaveRequestOverride item, double valueValue);
}

public sealed record FlaggedTieredLeaveRequestOverrideAdjuster(Registry Flags, string Flag, double Percent) : ITieredLeaveRequestOverrideFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredLeaveRequestOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTieredLeaveRequestOverrideAdjuster(List<double> Bounds, List<double> Rates) : ITieredLeaveRequestOverrideFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredLeaveRequestOverride item, double valueValue)
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

public sealed record RoundingTieredLeaveRequestOverrideAdjuster(double Step) : ITieredLeaveRequestOverrideFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredLeaveRequestOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapTieredLeaveRequestOverrideAdjuster(double Cap) : ITieredLeaveRequestOverrideFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredLeaveRequestOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the tiered leave request override steps in order and keeps a trace of each.</summary>
public sealed class TieredLeaveRequestOverrideEngine(List<ITieredLeaveRequestOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredLeaveRequestOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredLeaveRequestOverrideEngine DefaultTieredLeaveRequestOverrideEngine(Registry flags)
    {
        return new TieredLeaveRequestOverrideEngine(
            new List<ITieredLeaveRequestOverrideFactor>
            {
                new FlaggedTieredLeaveRequestOverrideAdjuster(flags, "tiered-leave-request-override-rolling", -6.0),
                new TieredTieredLeaveRequestOverrideAdjuster(
                    new List<double> { 5073.0, 5385.0, 5471.0, 8307.0 },
                    new List<double> { 0.81, 0.83, 0.91, 1.01 }),
                new RoundingTieredLeaveRequestOverrideAdjuster(0.1),
                new CapTieredLeaveRequestOverrideAdjuster(41291.0),
            },
            new List<string>(),
            0.2,
            14229.2);
    }

    public double Compute(TieredLeaveRequestOverride item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TieredLeaveRequestOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
