namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet adjustment is in its life cycle.</summary>
public enum TimesheetAdjustmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a timesheet adjustment and its current status.</summary>
public sealed class TimesheetAdjustment(string code, string title, string source, int sequence, double fte, TimesheetAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public int Sequence { get; set; } = sequence;
    public double Fte { get; set; } = fte;
    public TimesheetAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet adjustment in status requested.</summary>
    public static TimesheetAdjustment NewTimesheetAdjustment(string code, string title)
    {
        return new TimesheetAdjustment(code, title, "", 0, 0.0, TimesheetAdjustmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetAdjustmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetAdjustmentStatus next)
    {
        switch (Status)
        {
            case TimesheetAdjustmentStatus.Requested:
                switch (next)
                {
                    case TimesheetAdjustmentStatus.Revoked:
                    case TimesheetAdjustmentStatus.Expired:
                        return true;
                }
                return false;
            case TimesheetAdjustmentStatus.Granted:
                switch (next)
                {
                    case TimesheetAdjustmentStatus.Expired:
                        return true;
                }
                return false;
            case TimesheetAdjustmentStatus.Revoked:
                return next == TimesheetAdjustmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TimesheetAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedValue()
    {
        return Math.Round(Fte + (double)Sequence, 2);
    }

    public TimesheetAdjustment Copy()
    {
        return new TimesheetAdjustment(Code, Title, Source, Sequence, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
