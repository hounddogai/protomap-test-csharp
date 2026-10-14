namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional timesheet audit calculation.</summary>
public interface IRegionalTimesheetAuditStep
{
    string Describe();
    double Adjust(RegionalTimesheetAudit item, double valueValue);
}

public sealed record CapRegionalTimesheetAuditAdjuster(double Cap) : IRegionalTimesheetAuditStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalTimesheetAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRegionalTimesheetAuditAdjuster(double Percent) : IRegionalTimesheetAuditStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalTimesheetAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionalTimesheetAuditHoursOffsetAdjuster(double Weight) : IRegionalTimesheetAuditStep
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(RegionalTimesheetAudit item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record RoundingRegionalTimesheetAuditAdjuster(double Step) : IRegionalTimesheetAuditStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalTimesheetAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelRegionalTimesheetAuditAdjuster(Dictionary<string, double> Factors) : IRegionalTimesheetAuditStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RegionalTimesheetAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRegionalTimesheetAuditAdjuster(Registry Flags, string Flag, double Percent) : IRegionalTimesheetAuditStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalTimesheetAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional timesheet audit steps in order and keeps a trace of each.</summary>
public sealed class RegionalTimesheetAuditEngine(List<IRegionalTimesheetAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalTimesheetAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalTimesheetAuditEngine DefaultRegionalTimesheetAuditEngine(Registry flags)
    {
        return new RegionalTimesheetAuditEngine(
            new List<IRegionalTimesheetAuditStep>
            {
                new CapRegionalTimesheetAuditAdjuster(4084.0),
                new PercentRegionalTimesheetAuditAdjuster(7.9),
                new RegionalTimesheetAuditHoursOffsetAdjuster(0.74),
                new RoundingRegionalTimesheetAuditAdjuster(5.0),
                new LabelRegionalTimesheetAuditAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.08, ["retail"] = 0.9, ["north"] = 0.76 }),
                new FlaggedRegionalTimesheetAuditAdjuster(flags, "regional-timesheet-audit-legacy", 7.5),
            },
            new List<string>(),
            2.6,
            38921.0);
    }

    public double Compute(RegionalTimesheetAudit item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalTimesheetAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
