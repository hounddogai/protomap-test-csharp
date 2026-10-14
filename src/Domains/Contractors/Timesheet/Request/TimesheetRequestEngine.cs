namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet request calculation.</summary>
public interface ITimesheetRequestStep
{
    string Describe();
    double Adjust(TimesheetRequest item, double valueValue);
}

public sealed record FlaggedTimesheetRequestAdjuster(Registry Flags, string Flag, double Percent) : ITimesheetRequestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TimesheetRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTimesheetRequestAdjuster(double Cap) : ITimesheetRequestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTimesheetRequestAdjuster(double Minimum) : ITimesheetRequestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentTimesheetRequestAdjuster(double Percent) : ITimesheetRequestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the timesheet request steps in order and keeps a trace of each.</summary>
public sealed class TimesheetRequestEngine(List<ITimesheetRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetRequestEngine DefaultTimesheetRequestEngine(Registry flags)
    {
        return new TimesheetRequestEngine(
            new List<ITimesheetRequestStep>
            {
                new FlaggedTimesheetRequestAdjuster(flags, "timesheet-request-adaptive", -0.8),
                new CapTimesheetRequestAdjuster(19548.0),
                new MinimumTimesheetRequestAdjuster(25.2),
                new PercentTimesheetRequestAdjuster(8.4),
            },
            new List<string>(),
            0.2,
            87320.3);
    }

    public double Price(TimesheetRequest item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
