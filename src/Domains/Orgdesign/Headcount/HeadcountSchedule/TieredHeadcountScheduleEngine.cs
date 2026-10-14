namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered headcount schedule calculation.</summary>
public interface ITieredHeadcountScheduleAdjuster
{
    string Describe();
    double Adjust(TieredHeadcountSchedule item, double valueValue);
}

public sealed record MinimumTieredHeadcountScheduleAdjuster(double Minimum) : ITieredHeadcountScheduleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredHeadcountSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentTieredHeadcountScheduleAdjuster(double Percent) : ITieredHeadcountScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredHeadcountSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelTieredHeadcountScheduleAdjuster(Dictionary<string, double> Factors) : ITieredHeadcountScheduleAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(TieredHeadcountSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedTieredHeadcountScheduleAdjuster(Registry Flags, string Flag, double Percent) : ITieredHeadcountScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredHeadcountSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredHeadcountScheduleAdjuster(double Step) : ITieredHeadcountScheduleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredHeadcountSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tiered headcount schedule steps in order and keeps a trace of each.</summary>
public sealed class TieredHeadcountScheduleEngine(List<ITieredHeadcountScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredHeadcountScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredHeadcountScheduleEngine DefaultTieredHeadcountScheduleEngine(Registry flags)
    {
        return new TieredHeadcountScheduleEngine(
            new List<ITieredHeadcountScheduleAdjuster>
            {
                new MinimumTieredHeadcountScheduleAdjuster(4.1),
                new PercentTieredHeadcountScheduleAdjuster(-2.9),
                new ChannelTieredHeadcountScheduleAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.19, ["west"] = 1.06 }),
                new FlaggedTieredHeadcountScheduleAdjuster(flags, "tiered-headcount-schedule-pending", -1.9),
                new RoundingTieredHeadcountScheduleAdjuster(0.5),
            },
            new List<string>(),
            1.7,
            63965.0);
    }

    public double Run(TieredHeadcountSchedule item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredHeadcountSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
