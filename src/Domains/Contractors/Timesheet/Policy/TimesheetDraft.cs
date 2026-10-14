namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet draft is in its life cycle.</summary>
public enum TimesheetDraftStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a timesheet draft and its current status.</summary>
public sealed class TimesheetDraft(string code, string label, int graceDays, bool archived, bool active, double minimum, TimesheetDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public bool Archived { get; set; } = archived;
    public bool Active { get; set; } = active;
    public double Minimum { get; set; } = minimum;
    public TimesheetDraftStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet draft in status idle.</summary>
    public static TimesheetDraft NewTimesheetDraft(string code, string label)
    {
        return new TimesheetDraft(code, label, 0, false, false, 0.0, TimesheetDraftStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetDraftStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetDraftStatus next)
    {
        switch (Status)
        {
            case TimesheetDraftStatus.Idle:
                return next == TimesheetDraftStatus.Ready || next == TimesheetDraftStatus.Draining;
            case TimesheetDraftStatus.Warming:
                switch (next)
                {
                    case TimesheetDraftStatus.Draining:
                        return true;
                }
                return false;
            case TimesheetDraftStatus.Ready:
                return next == TimesheetDraftStatus.Draining;
            case TimesheetDraftStatus.Draining:
                return next == TimesheetDraftStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TimesheetDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"timesheet draft"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Minimum + (double)GraceDays, 2);
    }

    public TimesheetDraft Copy()
    {
        return new TimesheetDraft(Code, Label, GraceDays, Archived, Active, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
