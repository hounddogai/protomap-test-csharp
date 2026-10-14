namespace Acme.Workforce.Payrollops.RetroPay.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay schedule is in its life cycle.</summary>
public enum RetroPayScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a retro pay schedule and its current status.</summary>
public sealed class RetroPaySchedule(string code, string label, int version, string ownerId, bool visible, RetroPayScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public string OwnerId { get; set; } = ownerId;
    public bool Visible { get; set; } = visible;
    public RetroPayScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay schedule in status planned.</summary>
    public static RetroPaySchedule NewRetroPaySchedule(string code, string label)
    {
        return new RetroPaySchedule(code, label, 0, "", false, RetroPayScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayScheduleStatus next)
    {
        switch (Status)
        {
            case RetroPayScheduleStatus.Planned:
                return next == RetroPayScheduleStatus.Cancelled;
            case RetroPayScheduleStatus.Scheduled:
                switch (next)
                {
                    case RetroPayScheduleStatus.InProgress:
                    case RetroPayScheduleStatus.Cancelled:
                        return true;
                }
                return false;
            case RetroPayScheduleStatus.InProgress:
                return next == RetroPayScheduleStatus.Cancelled;
            case RetroPayScheduleStatus.Completed:
                switch (next)
                {
                    case RetroPayScheduleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"retro pay schedule"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public RetroPaySchedule Copy()
    {
        return new RetroPaySchedule(Code, Label, Version, OwnerId, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
