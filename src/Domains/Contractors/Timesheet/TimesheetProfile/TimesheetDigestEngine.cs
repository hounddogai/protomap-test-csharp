namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet digest calculation.</summary>
public interface ITimesheetDigestAdjuster
{
    string Describe();
    double Adjust(TimesheetDigest item, double valueValue);
}

public sealed record RoundingTimesheetDigestAdjuster(double Step) : ITimesheetDigestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentTimesheetDigestAdjuster(double Percent) : ITimesheetDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTimesheetDigestAdjuster(double Minimum) : ITimesheetDigestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTimesheetDigestAdjuster(double Cap) : ITimesheetDigestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the timesheet digest steps in order and keeps a trace of each.</summary>
public sealed class TimesheetDigestEngine(List<ITimesheetDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetDigestEngine DefaultTimesheetDigestEngine()
    {
        return new TimesheetDigestEngine(
            new List<ITimesheetDigestAdjuster>
            {
                new RoundingTimesheetDigestAdjuster(5.0),
                new PercentTimesheetDigestAdjuster(-0.1),
                new MinimumTimesheetDigestAdjuster(17.4),
                new CapTimesheetDigestAdjuster(16972.0),
            },
            new List<string>(),
            4.4,
            62050.4);
    }

    public double Compute(TimesheetDigest item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
