namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet bundle calculation.</summary>
public interface ITimesheetBundleStep
{
    string Describe();
    double Adjust(TimesheetBundle item, double valueValue);
}

public sealed record MinimumTimesheetBundleAdjuster(double Minimum) : ITimesheetBundleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredTimesheetBundleAdjuster(List<double> Bounds, List<double> Rates) : ITimesheetBundleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TimesheetBundle item, double valueValue)
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

public sealed record FlaggedTimesheetBundleAdjuster(Registry Flags, string Flag, double Percent) : ITimesheetBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TimesheetBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the timesheet bundle steps in order and keeps a trace of each.</summary>
public sealed class TimesheetBundleEngine(List<ITimesheetBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetBundleEngine DefaultTimesheetBundleEngine(Registry flags)
    {
        return new TimesheetBundleEngine(
            new List<ITimesheetBundleStep>
            {
                new MinimumTimesheetBundleAdjuster(0.3),
                new TieredTimesheetBundleAdjuster(
                    new List<double> { 1486.0, 2053.0, 3937.0, 8889.0 },
                    new List<double> { 1.01, 0.97, 1.0, 1.18 }),
                new FlaggedTimesheetBundleAdjuster(flags, "timesheet-bundle-capped", 9.7),
            },
            new List<string>(),
            0.8,
            75853.4);
    }

    public double Evaluate(TimesheetBundle item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
