namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global balance schedule calculation.</summary>
public interface IGlobalBalanceScheduleModifier
{
    string Describe();
    double Adjust(GlobalBalanceSchedule item, double valueValue);
}

public sealed record MinimumGlobalBalanceScheduleAdjuster(double Minimum) : IGlobalBalanceScheduleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalBalanceSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapGlobalBalanceScheduleAdjuster(double Cap) : IGlobalBalanceScheduleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalBalanceSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamGlobalBalanceScheduleAdjuster(Dictionary<string, double> Factors) : IGlobalBalanceScheduleModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(GlobalBalanceSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedGlobalBalanceScheduleAdjuster(Registry Flags, string Flag, double Percent) : IGlobalBalanceScheduleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalBalanceSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global balance schedule steps in order and keeps a trace of each.</summary>
public sealed class GlobalBalanceScheduleEngine(List<IGlobalBalanceScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalBalanceScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalBalanceScheduleEngine DefaultGlobalBalanceScheduleEngine(Registry flags)
    {
        return new GlobalBalanceScheduleEngine(
            new List<IGlobalBalanceScheduleModifier>
            {
                new MinimumGlobalBalanceScheduleAdjuster(26.1),
                new CapGlobalBalanceScheduleAdjuster(14951.0),
                new TeamGlobalBalanceScheduleAdjuster(new Dictionary<string, double> { ["retail"] = 1.33 }),
                new FlaggedGlobalBalanceScheduleAdjuster(flags, "global-balance-schedule-strict", 7.0),
            },
            new List<string>(),
            3.5,
            59302.2);
    }

    public double Evaluate(GlobalBalanceSchedule item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalBalanceSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
