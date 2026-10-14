namespace Acme.Workforce.Talent.Pool.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global pool snapshot is in its life cycle.</summary>
public enum GlobalPoolSnapshotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a global pool snapshot and its current status.</summary>
public sealed class GlobalPoolSnapshot(string code, string title, string unit, string segment, bool archived, int durationMinutes, GlobalPoolSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public string Segment { get; set; } = segment;
    public bool Archived { get; set; } = archived;
    public int DurationMinutes { get; set; } = durationMinutes;
    public GlobalPoolSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a global pool snapshot in status planned.</summary>
    public static GlobalPoolSnapshot NewGlobalPoolSnapshot(string code, string title)
    {
        return new GlobalPoolSnapshot(code, title, "", "", false, 0, GlobalPoolSnapshotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPoolSnapshotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPoolSnapshotStatus next)
    {
        switch (Status)
        {
            case GlobalPoolSnapshotStatus.Planned:
                switch (next)
                {
                    case GlobalPoolSnapshotStatus.InProgress:
                    case GlobalPoolSnapshotStatus.Completed:
                        return true;
                }
                return false;
            case GlobalPoolSnapshotStatus.Scheduled:
                return next == GlobalPoolSnapshotStatus.InProgress || next == GlobalPoolSnapshotStatus.Cancelled;
            case GlobalPoolSnapshotStatus.InProgress:
                return next == GlobalPoolSnapshotStatus.Completed;
            case GlobalPoolSnapshotStatus.Completed:
                switch (next)
                {
                    case GlobalPoolSnapshotStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalPoolSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global pool snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global pool snapshot"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public GlobalPoolSnapshot Copy()
    {
        return new GlobalPoolSnapshot(Code, Title, Unit, Segment, Archived, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
