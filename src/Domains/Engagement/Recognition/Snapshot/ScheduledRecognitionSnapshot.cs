namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled recognition snapshot is in its life cycle.</summary>
public enum ScheduledRecognitionSnapshotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One scheduled recognition snapshot, with what the planners need to act on it.</summary>
public sealed class ScheduledRecognitionSnapshot(string code, string title, int leaveDays, int priority, string costCenter, int attempts, string region, ScheduledRecognitionSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public int Priority { get; set; } = priority;
    public string CostCenter { get; set; } = costCenter;
    public int Attempts { get; set; } = attempts;
    public string Region { get; set; } = region;
    public ScheduledRecognitionSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled recognition snapshot in status planned.</summary>
    public static ScheduledRecognitionSnapshot NewScheduledRecognitionSnapshot(string code, string title)
    {
        return new ScheduledRecognitionSnapshot(
            code,
            title,
            0,
            0,
            "",
            0,
            "",
            ScheduledRecognitionSnapshotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledRecognitionSnapshotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledRecognitionSnapshotStatus next)
    {
        switch (Status)
        {
            case ScheduledRecognitionSnapshotStatus.Planned:
                switch (next)
                {
                    case ScheduledRecognitionSnapshotStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledRecognitionSnapshotStatus.Scheduled:
                return next == ScheduledRecognitionSnapshotStatus.Cancelled;
            case ScheduledRecognitionSnapshotStatus.InProgress:
                switch (next)
                {
                    case ScheduledRecognitionSnapshotStatus.Completed:
                    case ScheduledRecognitionSnapshotStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledRecognitionSnapshotStatus.Completed:
                return next == ScheduledRecognitionSnapshotStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledRecognitionSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled recognition snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)LeaveDays * (double)Priority, 2);
    }

    public ScheduledRecognitionSnapshot Copy()
    {
        return new ScheduledRecognitionSnapshot(
            Code,
            Title,
            LeaveDays,
            Priority,
            CostCenter,
            Attempts,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
