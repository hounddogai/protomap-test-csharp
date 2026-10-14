namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance schedule is in its life cycle.</summary>
public enum BalanceScheduleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a balance schedule and its current status.</summary>
public sealed class BalanceSchedule(string code, string title, double floorValue, double hours, int leaveDays, string unit, BalanceScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public double Hours { get; set; } = hours;
    public int LeaveDays { get; set; } = leaveDays;
    public string Unit { get; set; } = unit;
    public BalanceScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a balance schedule in status new.</summary>
    public static BalanceSchedule NewBalanceSchedule(string code, string title)
    {
        return new BalanceSchedule(code, title, 0.0, 0.0, 0, "", BalanceScheduleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceScheduleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceScheduleStatus next)
    {
        switch (Status)
        {
            case BalanceScheduleStatus.New:
                return next == BalanceScheduleStatus.Done;
            case BalanceScheduleStatus.Queued:
                switch (next)
                {
                    case BalanceScheduleStatus.Done:
                        return true;
                }
                return false;
            case BalanceScheduleStatus.Running:
                return next == BalanceScheduleStatus.Done || next == BalanceScheduleStatus.Failed;
            case BalanceScheduleStatus.Done:
                return next == BalanceScheduleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BalanceScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetLoad()
    {
        return Math.Round(FloorValue * (double)LeaveDays, 2);
    }

    public BalanceSchedule Copy()
    {
        return new BalanceSchedule(Code, Title, FloorValue, Hours, LeaveDays, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
