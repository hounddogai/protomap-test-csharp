namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled reorg schedule is in its life cycle.</summary>
public enum ScheduledReorgScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a scheduled reorg schedule and its current status.</summary>
public sealed class ScheduledReorgSchedule(string code, string label, string unit, string reviewer, int sequence, ScheduledReorgScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public string Reviewer { get; set; } = reviewer;
    public int Sequence { get; set; } = sequence;
    public ScheduledReorgScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled reorg schedule in status planned.</summary>
    public static ScheduledReorgSchedule NewScheduledReorgSchedule(string code, string label)
    {
        return new ScheduledReorgSchedule(code, label, "", "", 0, ScheduledReorgScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledReorgScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledReorgScheduleStatus next)
    {
        switch (Status)
        {
            case ScheduledReorgScheduleStatus.Planned:
                return next == ScheduledReorgScheduleStatus.Completed;
            case ScheduledReorgScheduleStatus.Scheduled:
                return next == ScheduledReorgScheduleStatus.InProgress;
            case ScheduledReorgScheduleStatus.InProgress:
                return next == ScheduledReorgScheduleStatus.Completed;
            case ScheduledReorgScheduleStatus.Completed:
                return next == ScheduledReorgScheduleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledReorgScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled reorg schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled reorg schedule"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public ScheduledReorgSchedule Copy()
    {
        return new ScheduledReorgSchedule(Code, Label, Unit, Reviewer, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
