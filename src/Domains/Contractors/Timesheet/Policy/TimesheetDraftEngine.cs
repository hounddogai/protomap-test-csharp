namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet draft calculation.</summary>
public interface ITimesheetDraftAdjuster
{
    string Describe();
    double Adjust(TimesheetDraft item, double valueValue);
}

public sealed record FlaggedTimesheetDraftAdjuster(Registry Flags, string Flag, double Percent) : ITimesheetDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TimesheetDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TimesheetDraftGraceDaysOffsetAdjuster(double Weight) : ITimesheetDraftAdjuster
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(TimesheetDraft item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record MinimumTimesheetDraftAdjuster(double Minimum) : ITimesheetDraftAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredTimesheetDraftAdjuster(List<double> Bounds, List<double> Rates) : ITimesheetDraftAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TimesheetDraft item, double valueValue)
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

/// <summary>Runs the timesheet draft steps in order and keeps a trace of each.</summary>
public sealed class TimesheetDraftEngine(List<ITimesheetDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetDraftEngine DefaultTimesheetDraftEngine(Registry flags)
    {
        return new TimesheetDraftEngine(
            new List<ITimesheetDraftAdjuster>
            {
                new FlaggedTimesheetDraftAdjuster(flags, "timesheet-draft-rolling", -4.9),
                new TimesheetDraftGraceDaysOffsetAdjuster(0.76),
                new MinimumTimesheetDraftAdjuster(23.9),
                new TieredTimesheetDraftAdjuster(
                    new List<double> { 3944.0, 4514.0, 8814.0 },
                    new List<double> { 0.9, 1.14, 0.94 }),
            },
            new List<string>(),
            3.4,
            54144.8);
    }

    public double Run(TimesheetDraft item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
