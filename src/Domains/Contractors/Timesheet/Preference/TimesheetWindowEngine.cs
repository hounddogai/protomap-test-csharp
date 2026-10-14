namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet window calculation.</summary>
public interface ITimesheetWindowFactor
{
    string Describe();
    double Adjust(TimesheetWindow item, double valueValue);
}

public sealed record TimesheetWindowVersionOffsetAdjuster(double Weight) : ITimesheetWindowFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(TimesheetWindow item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record TieredTimesheetWindowAdjuster(List<double> Bounds, List<double> Rates) : ITimesheetWindowFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TimesheetWindow item, double valueValue)
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

public sealed record CapTimesheetWindowAdjuster(double Cap) : ITimesheetWindowFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the timesheet window steps in order and keeps a trace of each.</summary>
public sealed class TimesheetWindowEngine(List<ITimesheetWindowFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetWindowFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetWindowEngine DefaultTimesheetWindowEngine()
    {
        return new TimesheetWindowEngine(
            new List<ITimesheetWindowFactor>
            {
                new TimesheetWindowVersionOffsetAdjuster(0.91),
                new TieredTimesheetWindowAdjuster(
                    new List<double> { 3403.0, 7726.0, 9057.0 },
                    new List<double> { 0.87, 1.08, 0.89 }),
                new CapTimesheetWindowAdjuster(25572.0),
            },
            new List<string>(),
            3.5,
            83873.1);
    }

    public double Compute(TimesheetWindow item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
