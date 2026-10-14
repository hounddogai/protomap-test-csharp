namespace Acme.Workforce.Contractors.Timesheet.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet record calculation.</summary>
public interface ITimesheetRecordAdjuster
{
    string Describe();
    double Adjust(TimesheetRecord item, double valueValue);
}

public sealed record MinimumTimesheetRecordAdjuster(double Minimum) : ITimesheetRecordAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingTimesheetRecordAdjuster(double Step) : ITimesheetRecordAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTimesheetRecordAdjuster(List<double> Bounds, List<double> Rates) : ITimesheetRecordAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TimesheetRecord item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

/// <summary>Runs the timesheet record steps in order and keeps a trace of each.</summary>
public sealed class TimesheetRecordEngine(List<ITimesheetRecordAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetRecordAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetRecordEngine DefaultTimesheetRecordEngine()
    {
        return new TimesheetRecordEngine(
            new List<ITimesheetRecordAdjuster>
            {
                new MinimumTimesheetRecordAdjuster(28.0),
                new RoundingTimesheetRecordAdjuster(0.05),
                new TieredTimesheetRecordAdjuster(
                    new List<double> { 1621.0, 4215.0, 6727.0, 8485.0 },
                    new List<double> { 0.9, 0.91, 1.0, 1.13 }),
            },
            new List<string>(),
            0.2,
            9959.6);
    }

    public double Run(TimesheetRecord item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
