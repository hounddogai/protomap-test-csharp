namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the refresh schedule calculation.</summary>
public interface IRefreshScheduleStep
{
    string Describe();
    double Adjust(RefreshSchedule item, double valueValue);
}

public sealed record PercentRefreshScheduleAdjuster(double Percent) : IRefreshScheduleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRefreshScheduleAdjuster(double Cap) : IRefreshScheduleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRefreshScheduleAdjuster(Registry Flags, string Flag, double Percent) : IRefreshScheduleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RefreshScheduleMaximumOffsetAdjuster(double Weight) : IRefreshScheduleStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(RefreshSchedule item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

/// <summary>Runs the refresh schedule steps in order and keeps a trace of each.</summary>
public sealed class RefreshScheduleEngine(List<IRefreshScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshScheduleEngine DefaultRefreshScheduleEngine(Registry flags)
    {
        return new RefreshScheduleEngine(
            new List<IRefreshScheduleStep>
            {
                new PercentRefreshScheduleAdjuster(14.1),
                new CapRefreshScheduleAdjuster(26042.0),
                new FlaggedRefreshScheduleAdjuster(flags, "refresh-schedule-legacy", 3.4),
                new RefreshScheduleMaximumOffsetAdjuster(1.11),
            },
            new List<string>(),
            3.6,
            34212.8);
    }

    public double Price(RefreshSchedule item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
