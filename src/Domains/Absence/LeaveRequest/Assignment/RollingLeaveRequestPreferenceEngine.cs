namespace Acme.Workforce.Absence.LeaveRequest.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling leave request preference calculation.</summary>
public interface IRollingLeaveRequestPreferenceStep
{
    string Describe();
    double Adjust(RollingLeaveRequestPreference item, double valueValue);
}

public sealed record RoundingRollingLeaveRequestPreferenceAdjuster(double Step) : IRollingLeaveRequestPreferenceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingLeaveRequestPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionRollingLeaveRequestPreferenceAdjuster(Dictionary<string, double> Factors) : IRollingLeaveRequestPreferenceStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(RollingLeaveRequestPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRollingLeaveRequestPreferenceAdjuster(double Percent) : IRollingLeaveRequestPreferenceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingLeaveRequestPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRollingLeaveRequestPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IRollingLeaveRequestPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingLeaveRequestPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RollingLeaveRequestPreferenceWindowDaysOffsetAdjuster(double Weight) : IRollingLeaveRequestPreferenceStep
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(RollingLeaveRequestPreference item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record MinimumRollingLeaveRequestPreferenceAdjuster(double Minimum) : IRollingLeaveRequestPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingLeaveRequestPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rolling leave request preference steps in order and keeps a trace of each.</summary>
public sealed class RollingLeaveRequestPreferenceEngine(List<IRollingLeaveRequestPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingLeaveRequestPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingLeaveRequestPreferenceEngine DefaultRollingLeaveRequestPreferenceEngine(Registry flags)
    {
        return new RollingLeaveRequestPreferenceEngine(
            new List<IRollingLeaveRequestPreferenceStep>
            {
                new RoundingRollingLeaveRequestPreferenceAdjuster(0.1),
                new RegionRollingLeaveRequestPreferenceAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.76, ["central"] = 1.05 }),
                new PercentRollingLeaveRequestPreferenceAdjuster(7.0),
                new FlaggedRollingLeaveRequestPreferenceAdjuster(
                    flags,
                    "rolling-leave-request-preference-adaptive",
                    8.6),
                new RollingLeaveRequestPreferenceWindowDaysOffsetAdjuster(1.97),
                new MinimumRollingLeaveRequestPreferenceAdjuster(48.3),
            },
            new List<string>(),
            4.0,
            75167.9);
    }

    public double Run(RollingLeaveRequestPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingLeaveRequestPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
