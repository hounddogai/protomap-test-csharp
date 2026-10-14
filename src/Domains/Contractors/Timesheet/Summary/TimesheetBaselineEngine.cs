namespace Acme.Workforce.Contractors.Timesheet.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet baseline calculation.</summary>
public interface ITimesheetBaselineFactor
{
    string Describe();
    double Adjust(TimesheetBaseline item, double valueValue);
}

public sealed record RoundingTimesheetBaselineAdjuster(double Step) : ITimesheetBaselineFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTimesheetBaselineAdjuster(double Minimum) : ITimesheetBaselineFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredTimesheetBaselineAdjuster(List<double> Bounds, List<double> Rates) : ITimesheetBaselineFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TimesheetBaseline item, double valueValue)
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

/// <summary>Runs the timesheet baseline steps in order and keeps a trace of each.</summary>
public sealed class TimesheetBaselineEngine(List<ITimesheetBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetBaselineEngine DefaultTimesheetBaselineEngine()
    {
        return new TimesheetBaselineEngine(
            new List<ITimesheetBaselineFactor>
            {
                new RoundingTimesheetBaselineAdjuster(0.5),
                new MinimumTimesheetBaselineAdjuster(41.2),
                new TieredTimesheetBaselineAdjuster(
                    new List<double> { 1912.0, 2664.0 },
                    new List<double> { 0.88, 1.13 }),
            },
            new List<string>(),
            4.2,
            76014.6);
    }

    public double Compute(TimesheetBaseline item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
