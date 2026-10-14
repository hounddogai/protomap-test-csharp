namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual schedule is in its life cycle.</summary>
public enum AccrualScheduleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One accrual schedule, with what the planners need to act on it.</summary>
public sealed class AccrualSchedule(string code, string label, string grade, string title, double fte, AccrualScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public AccrualScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a accrual schedule in status idle.</summary>
    public static AccrualSchedule NewAccrualSchedule(string code, string label)
    {
        return new AccrualSchedule(code, label, "", "", 0.0, AccrualScheduleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualScheduleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualScheduleStatus next)
    {
        switch (Status)
        {
            case AccrualScheduleStatus.Idle:
                return next == AccrualScheduleStatus.Draining || next == AccrualScheduleStatus.Stopped;
            case AccrualScheduleStatus.Warming:
                return next == AccrualScheduleStatus.Ready || next == AccrualScheduleStatus.Draining;
            case AccrualScheduleStatus.Ready:
                return next == AccrualScheduleStatus.Draining || next == AccrualScheduleStatus.Stopped;
            case AccrualScheduleStatus.Draining:
                return next == AccrualScheduleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AccrualScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public AccrualSchedule Copy()
    {
        return new AccrualSchedule(Code, Label, Grade, Title, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
