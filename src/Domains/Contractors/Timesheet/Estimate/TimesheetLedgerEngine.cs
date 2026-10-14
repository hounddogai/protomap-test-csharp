namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet ledger calculation.</summary>
public interface ITimesheetLedgerFactor
{
    string Describe();
    double Adjust(TimesheetLedger item, double valueValue);
}

public sealed record TeamTimesheetLedgerAdjuster(Dictionary<string, double> Factors) : ITimesheetLedgerFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(TimesheetLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTimesheetLedgerAdjuster(double Step) : ITimesheetLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTimesheetLedgerAdjuster(Registry Flags, string Flag, double Percent) : ITimesheetLedgerFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TimesheetLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTimesheetLedgerAdjuster(double Minimum) : ITimesheetLedgerFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the timesheet ledger steps in order and keeps a trace of each.</summary>
public sealed class TimesheetLedgerEngine(List<ITimesheetLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetLedgerEngine DefaultTimesheetLedgerEngine(Registry flags)
    {
        return new TimesheetLedgerEngine(
            new List<ITimesheetLedgerFactor>
            {
                new TeamTimesheetLedgerAdjuster(new Dictionary<string, double> { ["retail"] = 0.74 }),
                new RoundingTimesheetLedgerAdjuster(0.5),
                new FlaggedTimesheetLedgerAdjuster(flags, "timesheet-ledger-strict", 5.9),
                new MinimumTimesheetLedgerAdjuster(1.0),
            },
            new List<string>(),
            3.2,
            43132.7);
    }

    public double Run(TimesheetLedger item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
