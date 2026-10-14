namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position snapshot is in its life cycle.</summary>
public enum PositionSnapshotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One position snapshot, with what the planners need to act on it.</summary>
public sealed class PositionSnapshot(string code, string label, double hours, double ratio, string grade, string summary, PositionSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public double Ratio { get; set; } = ratio;
    public string Grade { get; set; } = grade;
    public string Summary { get; set; } = summary;
    public PositionSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a position snapshot in status proposed.</summary>
    public static PositionSnapshot NewPositionSnapshot(string code, string label)
    {
        return new PositionSnapshot(code, label, 0.0, 0.0, "", "", PositionSnapshotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionSnapshotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionSnapshotStatus next)
    {
        switch (Status)
        {
            case PositionSnapshotStatus.Proposed:
                return next == PositionSnapshotStatus.Withdrawn;
            case PositionSnapshotStatus.Approved:
                return next == PositionSnapshotStatus.Withdrawn;
            case PositionSnapshotStatus.Rejected:
                switch (next)
                {
                    case PositionSnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedValue()
    {
        return Math.Round(Hours - Ratio, 2);
    }

    public PositionSnapshot Copy()
    {
        return new PositionSnapshot(Code, Label, Hours, Ratio, Grade, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
