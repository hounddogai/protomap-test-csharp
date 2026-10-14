namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor snapshot is in its life cycle.</summary>
public enum VisitorSnapshotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a visitor snapshot and its current status.</summary>
public sealed class VisitorSnapshot(string code, string label, int version, double score, int attempts, string contactEmail, int quantity, VisitorSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public double Score { get; set; } = score;
    public int Attempts { get; set; } = attempts;
    public string ContactEmail { get; set; } = contactEmail;
    public int Quantity { get; set; } = quantity;
    public VisitorSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a visitor snapshot in status idle.</summary>
    public static VisitorSnapshot NewVisitorSnapshot(string code, string label)
    {
        return new VisitorSnapshot(code, label, 0, 0.0, 0, "", 0, VisitorSnapshotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorSnapshotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorSnapshotStatus next)
    {
        switch (Status)
        {
            case VisitorSnapshotStatus.Idle:
                return next == VisitorSnapshotStatus.Draining || next == VisitorSnapshotStatus.Stopped;
            case VisitorSnapshotStatus.Warming:
                switch (next)
                {
                    case VisitorSnapshotStatus.Draining:
                    case VisitorSnapshotStatus.Stopped:
                        return true;
                }
                return false;
            case VisitorSnapshotStatus.Ready:
                switch (next)
                {
                    case VisitorSnapshotStatus.Draining:
                    case VisitorSnapshotStatus.Stopped:
                        return true;
                }
                return false;
            case VisitorSnapshotStatus.Draining:
                return next == VisitorSnapshotStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(VisitorSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Version - Score, 2);
    }

    public VisitorSnapshot Copy()
    {
        return new VisitorSnapshot(Code, Label, Version, Score, Attempts, ContactEmail, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
