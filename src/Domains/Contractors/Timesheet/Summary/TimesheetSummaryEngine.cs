namespace Acme.Workforce.Contractors.Timesheet.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet summary calculation.</summary>
public interface ITimesheetSummaryAdjuster
{
    string Describe();
    double Adjust(TimesheetSummary item, double valueValue);
}

public sealed record TieredTimesheetSummaryAdjuster(List<double> Bounds, List<double> Rates) : ITimesheetSummaryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TimesheetSummary item, double valueValue)
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

public sealed record MinimumTimesheetSummaryAdjuster(double Minimum) : ITimesheetSummaryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingTimesheetSummaryAdjuster(double Step) : ITimesheetSummaryAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTimesheetSummaryAdjuster(Registry Flags, string Flag, double Percent) : ITimesheetSummaryAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TimesheetSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentTimesheetSummaryAdjuster(double Percent) : ITimesheetSummaryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTimesheetSummaryAdjuster(double Cap) : ITimesheetSummaryAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the timesheet summary steps in order and keeps a trace of each.</summary>
public sealed class TimesheetSummaryEngine(List<ITimesheetSummaryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetSummaryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetSummaryEngine DefaultTimesheetSummaryEngine(Registry flags)
    {
        return new TimesheetSummaryEngine(
            new List<ITimesheetSummaryAdjuster>
            {
                new TieredTimesheetSummaryAdjuster(
                    new List<double> { 3809.0, 3967.0, 5246.0, 8939.0 },
                    new List<double> { 0.85, 1.06, 0.98, 1.06 }),
                new MinimumTimesheetSummaryAdjuster(1.7),
                new RoundingTimesheetSummaryAdjuster(0.05),
                new FlaggedTimesheetSummaryAdjuster(flags, "timesheet-summary-tiered", -4.1),
                new PercentTimesheetSummaryAdjuster(23.2),
                new CapTimesheetSummaryAdjuster(12819.0),
            },
            new List<string>(),
            1.2,
            40348.2);
    }

    public double Evaluate(TimesheetSummary item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
