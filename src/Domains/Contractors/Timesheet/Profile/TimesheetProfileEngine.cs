namespace Acme.Workforce.Contractors.Timesheet.Profile;

/// <summary>One step of the timesheet profile calculation.</summary>
public interface ITimesheetProfileFactor
{
    string Describe();
    double Adjust(TimesheetProfile item, double valueValue);
}

public sealed record CapTimesheetProfileAdjuster(double Cap) : ITimesheetProfileFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTimesheetProfileAdjuster(double Percent) : ITimesheetProfileFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TimesheetProfileExpiresAtOffsetAdjuster(double Weight) : ITimesheetProfileFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(TimesheetProfile item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

/// <summary>Runs the timesheet profile steps in order and keeps a trace of each.</summary>
public sealed class TimesheetProfileEngine(List<ITimesheetProfileFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetProfileFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetProfileEngine DefaultTimesheetProfileEngine()
    {
        return new TimesheetProfileEngine(
            new List<ITimesheetProfileFactor>
            {
                new CapTimesheetProfileAdjuster(43218.0),
                new PercentTimesheetProfileAdjuster(12.8),
                new TimesheetProfileExpiresAtOffsetAdjuster(0.21),
            },
            new List<string>(),
            1.0,
            7708.4);
    }

    public double Run(TimesheetProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
