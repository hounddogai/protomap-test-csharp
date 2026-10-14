namespace Acme.Workforce.Payrollops.PayRun.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient pay run schedule is in its life cycle.</summary>
public enum LenientPayRunScheduleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One lenient pay run schedule, with what the planners need to act on it.</summary>
public sealed class LenientPayRunSchedule(string code, string label, bool active, double amount, string title, string department, double fte, LenientPayRunScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public double Amount { get; set; } = amount;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public double Fte { get; set; } = fte;
    public LenientPayRunScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient pay run schedule in status open.</summary>
    public static LenientPayRunSchedule NewLenientPayRunSchedule(string code, string label)
    {
        return new LenientPayRunSchedule(code, label, false, 0.0, "", "", 0.0, LenientPayRunScheduleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPayRunScheduleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPayRunScheduleStatus next)
    {
        switch (Status)
        {
            case LenientPayRunScheduleStatus.Open:
                switch (next)
                {
                    case LenientPayRunScheduleStatus.Resolved:
                    case LenientPayRunScheduleStatus.Reopened:
                        return true;
                }
                return false;
            case LenientPayRunScheduleStatus.InReview:
                return next == LenientPayRunScheduleStatus.Reopened || next == LenientPayRunScheduleStatus.Archived;
            case LenientPayRunScheduleStatus.Resolved:
                switch (next)
                {
                    case LenientPayRunScheduleStatus.Reopened:
                    case LenientPayRunScheduleStatus.Archived:
                        return true;
                }
                return false;
            case LenientPayRunScheduleStatus.Reopened:
                switch (next)
                {
                    case LenientPayRunScheduleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientPayRunScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient pay run schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetTotal()
    {
        return Math.Round(Fte * Amount, 2);
    }

    public LenientPayRunSchedule Copy()
    {
        return new LenientPayRunSchedule(Code, Label, Active, Amount, Title, Department, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
