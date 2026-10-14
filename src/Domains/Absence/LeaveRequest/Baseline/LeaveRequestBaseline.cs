namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request baseline is in its life cycle.</summary>
public enum LeaveRequestBaselineStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One leave request baseline, with what the planners need to act on it.</summary>
public sealed class LeaveRequestBaseline(string code, string title, string currency, int leaveDays, double threshold, LeaveRequestBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public int LeaveDays { get; set; } = leaveDays;
    public double Threshold { get; set; } = threshold;
    public LeaveRequestBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a leave request baseline in status new.</summary>
    public static LeaveRequestBaseline NewLeaveRequestBaseline(string code, string title)
    {
        return new LeaveRequestBaseline(code, title, "", 0, 0.0, LeaveRequestBaselineStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestBaselineStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestBaselineStatus next)
    {
        switch (Status)
        {
            case LeaveRequestBaselineStatus.New:
                switch (next)
                {
                    case LeaveRequestBaselineStatus.Queued:
                    case LeaveRequestBaselineStatus.Done:
                        return true;
                }
                return false;
            case LeaveRequestBaselineStatus.Queued:
                switch (next)
                {
                    case LeaveRequestBaselineStatus.Running:
                    case LeaveRequestBaselineStatus.Failed:
                        return true;
                }
                return false;
            case LeaveRequestBaselineStatus.Running:
                return next == LeaveRequestBaselineStatus.Done || next == LeaveRequestBaselineStatus.Failed;
            case LeaveRequestBaselineStatus.Done:
                switch (next)
                {
                    case LeaveRequestBaselineStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)LeaveDays * Threshold, 2);
    }

    public LeaveRequestBaseline Copy()
    {
        return new LeaveRequestBaseline(Code, Title, Currency, LeaveDays, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
