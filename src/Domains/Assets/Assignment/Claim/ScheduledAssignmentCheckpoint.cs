namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled assignment checkpoint is in its life cycle.</summary>
public enum ScheduledAssignmentCheckpointStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One scheduled assignment checkpoint, with what the planners need to act on it.</summary>
public sealed class ScheduledAssignmentCheckpoint(string code, string title, string source, string managerId, int sequence, ScheduledAssignmentCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public string ManagerId { get; set; } = managerId;
    public int Sequence { get; set; } = sequence;
    public ScheduledAssignmentCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled assignment checkpoint in status new.</summary>
    public static ScheduledAssignmentCheckpoint NewScheduledAssignmentCheckpoint(string code, string title)
    {
        return new ScheduledAssignmentCheckpoint(code, title, "", "", 0, ScheduledAssignmentCheckpointStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAssignmentCheckpointStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAssignmentCheckpointStatus next)
    {
        switch (Status)
        {
            case ScheduledAssignmentCheckpointStatus.New:
                return next == ScheduledAssignmentCheckpointStatus.Running || next == ScheduledAssignmentCheckpointStatus.Done;
            case ScheduledAssignmentCheckpointStatus.Queued:
                switch (next)
                {
                    case ScheduledAssignmentCheckpointStatus.Done:
                        return true;
                }
                return false;
            case ScheduledAssignmentCheckpointStatus.Running:
                return next == ScheduledAssignmentCheckpointStatus.Done || next == ScheduledAssignmentCheckpointStatus.Failed;
            case ScheduledAssignmentCheckpointStatus.Done:
                switch (next)
                {
                    case ScheduledAssignmentCheckpointStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledAssignmentCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled assignment checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled assignment checkpoint"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public ScheduledAssignmentCheckpoint Copy()
    {
        return new ScheduledAssignmentCheckpoint(Code, Title, Source, ManagerId, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
