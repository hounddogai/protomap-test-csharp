namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual schedule calculation.</summary>
public interface IAccrualScheduleStep
{
    string Describe();
    double Adjust(AccrualSchedule item, double valueValue);
}

public sealed record RoundingAccrualScheduleAdjuster(double Step) : IAccrualScheduleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelAccrualScheduleAdjuster(Dictionary<string, double> Factors) : IAccrualScheduleStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AccrualSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAccrualScheduleAdjuster(Registry Flags, string Flag, double Percent) : IAccrualScheduleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccrualSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAccrualScheduleAdjuster(double Cap) : IAccrualScheduleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the accrual schedule steps in order and keeps a trace of each.</summary>
public sealed class AccrualScheduleEngine(List<IAccrualScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualScheduleEngine DefaultAccrualScheduleEngine(Registry flags)
    {
        return new AccrualScheduleEngine(
            new List<IAccrualScheduleStep>
            {
                new RoundingAccrualScheduleAdjuster(0.5),
                new LabelAccrualScheduleAdjuster(new Dictionary<string, double> { ["online"] = 1.15 }),
                new FlaggedAccrualScheduleAdjuster(flags, "accrual-schedule-capped", 2.8),
                new CapAccrualScheduleAdjuster(23112.0),
            },
            new List<string>(),
            2.7,
            91897.2);
    }

    public double Price(AccrualSchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<AccrualSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
