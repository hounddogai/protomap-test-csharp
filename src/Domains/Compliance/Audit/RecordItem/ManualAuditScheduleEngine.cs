namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual audit schedule calculation.</summary>
public interface IManualAuditScheduleAdjuster
{
    string Describe();
    double Adjust(ManualAuditSchedule item, double valueValue);
}

public sealed record MinimumManualAuditScheduleAdjuster(double Minimum) : IManualAuditScheduleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualAuditSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingManualAuditScheduleAdjuster(double Step) : IManualAuditScheduleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualAuditSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedManualAuditScheduleAdjuster(Registry Flags, string Flag, double Percent) : IManualAuditScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualAuditSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual audit schedule steps in order and keeps a trace of each.</summary>
public sealed class ManualAuditScheduleEngine(List<IManualAuditScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualAuditScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualAuditScheduleEngine DefaultManualAuditScheduleEngine(Registry flags)
    {
        return new ManualAuditScheduleEngine(
            new List<IManualAuditScheduleAdjuster>
            {
                new MinimumManualAuditScheduleAdjuster(46.2),
                new RoundingManualAuditScheduleAdjuster(1.0),
                new FlaggedManualAuditScheduleAdjuster(flags, "manual-audit-schedule-default", -2.2),
            },
            new List<string>(),
            1.9,
            31431.6);
    }

    public double Run(ManualAuditSchedule item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ManualAuditSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
