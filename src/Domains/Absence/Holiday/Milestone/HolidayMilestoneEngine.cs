namespace Acme.Workforce.Absence.Holiday.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday milestone calculation.</summary>
public interface IHolidayMilestoneAdjuster
{
    string Describe();
    double Adjust(HolidayMilestone item, double valueValue);
}

public sealed record PercentHolidayMilestoneAdjuster(double Percent) : IHolidayMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapHolidayMilestoneAdjuster(double Cap) : IHolidayMilestoneAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HolidayMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedHolidayMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IHolidayMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HolidayMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleHolidayMilestoneAdjuster(Dictionary<string, double> Factors) : IHolidayMilestoneAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(HolidayMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumHolidayMilestoneAdjuster(double Minimum) : IHolidayMilestoneAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidayMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingHolidayMilestoneAdjuster(double Step) : IHolidayMilestoneAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the holiday milestone steps in order and keeps a trace of each.</summary>
public sealed class HolidayMilestoneEngine(List<IHolidayMilestoneAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayMilestoneAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayMilestoneEngine DefaultHolidayMilestoneEngine(Registry flags)
    {
        return new HolidayMilestoneEngine(
            new List<IHolidayMilestoneAdjuster>
            {
                new PercentHolidayMilestoneAdjuster(-10.8),
                new CapHolidayMilestoneAdjuster(33474.0),
                new FlaggedHolidayMilestoneAdjuster(flags, "holiday-milestone-strict", -6.8),
                new LocaleHolidayMilestoneAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.16, ["east"] = 1.38, ["west"] = 0.71 }),
                new MinimumHolidayMilestoneAdjuster(14.8),
                new RoundingHolidayMilestoneAdjuster(1.0),
            },
            new List<string>(),
            3.7,
            95390.1);
    }

    public double Evaluate(HolidayMilestone item)
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

    public Dictionary<string, double> EvaluateAll(List<HolidayMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
