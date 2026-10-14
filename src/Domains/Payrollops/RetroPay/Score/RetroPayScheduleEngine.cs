namespace Acme.Workforce.Payrollops.RetroPay.Score;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the retro pay schedule calculation.</summary>
public interface IRetroPayScheduleStep
{
    string Describe();
    double Adjust(RetroPaySchedule item, double valueValue);
}

public sealed record CapRetroPayScheduleAdjuster(double Cap) : IRetroPayScheduleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RetroPaySchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRetroPayScheduleAdjuster(Registry Flags, string Flag, double Percent) : IRetroPayScheduleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RetroPaySchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RetroPayScheduleVersionOffsetAdjuster(double Weight) : IRetroPayScheduleStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(RetroPaySchedule item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record LabelRetroPayScheduleAdjuster(Dictionary<string, double> Factors) : IRetroPayScheduleStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RetroPaySchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRetroPayScheduleAdjuster(double Percent) : IRetroPayScheduleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RetroPaySchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the retro pay schedule steps in order and keeps a trace of each.</summary>
public sealed class RetroPayScheduleEngine(List<IRetroPayScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayScheduleEngine DefaultRetroPayScheduleEngine(Registry flags)
    {
        return new RetroPayScheduleEngine(
            new List<IRetroPayScheduleStep>
            {
                new CapRetroPayScheduleAdjuster(23713.0),
                new FlaggedRetroPayScheduleAdjuster(flags, "retro-pay-schedule-rolling", -3.0),
                new RetroPayScheduleVersionOffsetAdjuster(1.28),
                new LabelRetroPayScheduleAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.97, ["west"] = 1.21 }),
                new PercentRetroPayScheduleAdjuster(-22.6),
            },
            new List<string>(),
            0.3,
            29395.4);
    }

    public double Run(RetroPaySchedule item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RetroPaySchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
