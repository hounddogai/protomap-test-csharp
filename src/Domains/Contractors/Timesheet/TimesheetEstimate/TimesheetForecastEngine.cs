namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet forecast calculation.</summary>
public interface ITimesheetForecastAdjuster
{
    string Describe();
    double Adjust(TimesheetForecast item, double valueValue);
}

public sealed record MinimumTimesheetForecastAdjuster(double Minimum) : ITimesheetForecastAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TimesheetForecastCapacityOffsetAdjuster(double Weight) : ITimesheetForecastAdjuster
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(TimesheetForecast item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record RoundingTimesheetForecastAdjuster(double Step) : ITimesheetForecastAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTimesheetForecastAdjuster(Registry Flags, string Flag, double Percent) : ITimesheetForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TimesheetForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentTimesheetForecastAdjuster(double Percent) : ITimesheetForecastAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTimesheetForecastAdjuster(double Cap) : ITimesheetForecastAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the timesheet forecast steps in order and keeps a trace of each.</summary>
public sealed class TimesheetForecastEngine(List<ITimesheetForecastAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetForecastAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetForecastEngine DefaultTimesheetForecastEngine(Registry flags)
    {
        return new TimesheetForecastEngine(
            new List<ITimesheetForecastAdjuster>
            {
                new MinimumTimesheetForecastAdjuster(25.5),
                new TimesheetForecastCapacityOffsetAdjuster(1.11),
                new RoundingTimesheetForecastAdjuster(0.25),
                new FlaggedTimesheetForecastAdjuster(flags, "timesheet-forecast-seasonal", -3.9),
                new PercentTimesheetForecastAdjuster(10.5),
                new CapTimesheetForecastAdjuster(43519.0),
            },
            new List<string>(),
            2.5,
            17962.2);
    }

    public double Price(TimesheetForecast item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
