namespace Acme.Workforce.Contractors.Timesheet.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet reservation calculation.</summary>
public interface ITimesheetReservationFactor
{
    string Describe();
    double Adjust(TimesheetReservation item, double valueValue);
}

public sealed record TimesheetReservationScoreOffsetAdjuster(double Weight) : ITimesheetReservationFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(TimesheetReservation item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record LabelTimesheetReservationAdjuster(Dictionary<string, double> Factors) : ITimesheetReservationFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TimesheetReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTimesheetReservationAdjuster(double Step) : ITimesheetReservationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTimesheetReservationAdjuster(double Minimum) : ITimesheetReservationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTimesheetReservationAdjuster(double Cap) : ITimesheetReservationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the timesheet reservation steps in order and keeps a trace of each.</summary>
public sealed class TimesheetReservationEngine(List<ITimesheetReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetReservationEngine DefaultTimesheetReservationEngine()
    {
        return new TimesheetReservationEngine(
            new List<ITimesheetReservationFactor>
            {
                new TimesheetReservationScoreOffsetAdjuster(1.06),
                new LabelTimesheetReservationAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.0, ["north"] = 0.74, ["retail"] = 1.38 }),
                new RoundingTimesheetReservationAdjuster(0.25),
                new MinimumTimesheetReservationAdjuster(9.2),
                new CapTimesheetReservationAdjuster(36945.0),
            },
            new List<string>(),
            3.2,
            44544.6);
    }

    public double Compute(TimesheetReservation item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
