namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet cycle is in its life cycle.</summary>
public enum TimesheetCycleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A timesheet cycle as the timesheet team keeps it.</summary>
public sealed class TimesheetCycle(string code, string label, string channel, Dictionary<string, string> attributes, string salaryBand, double amount, int windowDays, string locale, double minimum, string checksum, double threshold, TimesheetCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string SalaryBand { get; set; } = salaryBand;
    public double Amount { get; set; } = amount;
    public int WindowDays { get; set; } = windowDays;
    public string Locale { get; set; } = locale;
    public double Minimum { get; set; } = minimum;
    public string Checksum { get; set; } = checksum;
    public double Threshold { get; set; } = threshold;
    public TimesheetCycleStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet cycle in status new.</summary>
    public static TimesheetCycle NewTimesheetCycle(string code, string label)
    {
        return new TimesheetCycle(
            code,
            label,
            "",
            new Dictionary<string, string>(),
            "",
            0.0,
            0,
            "",
            0.0,
            "",
            0.0,
            TimesheetCycleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetCycleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetCycleStatus next)
    {
        switch (Status)
        {
            case TimesheetCycleStatus.New:
                return next == TimesheetCycleStatus.Queued;
            case TimesheetCycleStatus.Queued:
                return next == TimesheetCycleStatus.Failed;
            case TimesheetCycleStatus.Running:
                switch (next)
                {
                    case TimesheetCycleStatus.Failed:
                        return true;
                }
                return false;
            case TimesheetCycleStatus.Done:
                return next == TimesheetCycleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(TimesheetCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedLoad()
    {
        return Math.Round(Amount - Threshold, 2);
    }

    public TimesheetCycle Copy()
    {
        return new TimesheetCycle(
            Code,
            Label,
            Channel,
            Attributes,
            SalaryBand,
            Amount,
            WindowDays,
            Locale,
            Minimum,
            Checksum,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
