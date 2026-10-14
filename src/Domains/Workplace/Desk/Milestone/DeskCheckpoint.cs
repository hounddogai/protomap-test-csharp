namespace Acme.Workforce.Workplace.Desk.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk checkpoint is in its life cycle.</summary>
public enum DeskCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A desk checkpoint. Its code is unique within a tenant.</summary>
public sealed class DeskCheckpoint(string code, string label, bool archived, string segment, double maximum, DeskCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string Segment { get; set; } = segment;
    public double Maximum { get; set; } = maximum;
    public DeskCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a desk checkpoint in status idle.</summary>
    public static DeskCheckpoint NewDeskCheckpoint(string code, string label)
    {
        return new DeskCheckpoint(code, label, false, "", 0.0, DeskCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskCheckpointStatus next)
    {
        switch (Status)
        {
            case DeskCheckpointStatus.Idle:
                switch (next)
                {
                    case DeskCheckpointStatus.Ready:
                    case DeskCheckpointStatus.Draining:
                        return true;
                }
                return false;
            case DeskCheckpointStatus.Warming:
                return next == DeskCheckpointStatus.Ready || next == DeskCheckpointStatus.Draining;
            case DeskCheckpointStatus.Ready:
                return next == DeskCheckpointStatus.Stopped;
            case DeskCheckpointStatus.Draining:
                switch (next)
                {
                    case DeskCheckpointStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public DeskCheckpoint Copy()
    {
        return new DeskCheckpoint(Code, Label, Archived, Segment, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
