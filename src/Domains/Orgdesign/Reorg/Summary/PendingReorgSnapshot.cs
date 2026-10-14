namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending reorg snapshot is in its life cycle.</summary>
public enum PendingReorgSnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One pending reorg snapshot, with what the planners need to act on it.</summary>
public sealed class PendingReorgSnapshot(string code, string label, double ceiling, double margin, string country, string name, string channel, PendingReorgSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public double Margin { get; set; } = margin;
    public string Country { get; set; } = country;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public PendingReorgSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a pending reorg snapshot in status new.</summary>
    public static PendingReorgSnapshot NewPendingReorgSnapshot(string code, string label)
    {
        return new PendingReorgSnapshot(code, label, 0.0, 0.0, "", "", "", PendingReorgSnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingReorgSnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingReorgSnapshotStatus next)
    {
        switch (Status)
        {
            case PendingReorgSnapshotStatus.New:
                return next == PendingReorgSnapshotStatus.Running || next == PendingReorgSnapshotStatus.Failed;
            case PendingReorgSnapshotStatus.Queued:
                switch (next)
                {
                    case PendingReorgSnapshotStatus.Failed:
                        return true;
                }
                return false;
            case PendingReorgSnapshotStatus.Running:
                return next == PendingReorgSnapshotStatus.Done || next == PendingReorgSnapshotStatus.Failed;
            case PendingReorgSnapshotStatus.Done:
                return next == PendingReorgSnapshotStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingReorgSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending reorg snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pending reorg snapshot"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Margin * Ceiling, 2);
    }

    public PendingReorgSnapshot Copy()
    {
        return new PendingReorgSnapshot(Code, Label, Ceiling, Margin, Country, Name, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
