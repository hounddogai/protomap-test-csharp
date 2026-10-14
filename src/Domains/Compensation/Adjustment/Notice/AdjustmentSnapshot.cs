namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment snapshot is in its life cycle.</summary>
public enum AdjustmentSnapshotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a adjustment snapshot and its current status.</summary>
public sealed class AdjustmentSnapshot(string code, string label, double maximum, int batchSize, double floorValue, string name, double discount, AdjustmentSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public int BatchSize { get; set; } = batchSize;
    public double FloorValue { get; set; } = floorValue;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public AdjustmentSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment snapshot in status open.</summary>
    public static AdjustmentSnapshot NewAdjustmentSnapshot(string code, string label)
    {
        return new AdjustmentSnapshot(code, label, 0.0, 0, 0.0, "", 0.0, AdjustmentSnapshotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentSnapshotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentSnapshotStatus next)
    {
        switch (Status)
        {
            case AdjustmentSnapshotStatus.Open:
                return next == AdjustmentSnapshotStatus.Reopened;
            case AdjustmentSnapshotStatus.InReview:
                return next == AdjustmentSnapshotStatus.Resolved;
            case AdjustmentSnapshotStatus.Resolved:
                return next == AdjustmentSnapshotStatus.Reopened || next == AdjustmentSnapshotStatus.Archived;
            case AdjustmentSnapshotStatus.Reopened:
                return next == AdjustmentSnapshotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdjustmentSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adjustment snapshot"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(FloorValue - (double)BatchSize, 2);
    }

    public AdjustmentSnapshot Copy()
    {
        return new AdjustmentSnapshot(Code, Label, Maximum, BatchSize, FloorValue, Name, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
