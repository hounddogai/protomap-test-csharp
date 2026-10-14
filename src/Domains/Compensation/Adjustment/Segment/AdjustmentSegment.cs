namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment segment is in its life cycle.</summary>
public enum AdjustmentSegmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a adjustment segment and its current status.</summary>
public sealed class AdjustmentSegment(string code, string label, int capacity, string category, bool archived, int startsAt, AdjustmentSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public string Category { get; set; } = category;
    public bool Archived { get; set; } = archived;
    public int StartsAt { get; set; } = startsAt;
    public AdjustmentSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment segment in status new.</summary>
    public static AdjustmentSegment NewAdjustmentSegment(string code, string label)
    {
        return new AdjustmentSegment(code, label, 0, "", false, 0, AdjustmentSegmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentSegmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentSegmentStatus next)
    {
        switch (Status)
        {
            case AdjustmentSegmentStatus.New:
                return next == AdjustmentSegmentStatus.Queued;
            case AdjustmentSegmentStatus.Queued:
                return next == AdjustmentSegmentStatus.Done;
            case AdjustmentSegmentStatus.Running:
                return next == AdjustmentSegmentStatus.Failed;
            case AdjustmentSegmentStatus.Done:
                return next == AdjustmentSegmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdjustmentSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)StartsAt * (double)Capacity, 2);
    }

    public AdjustmentSegment Copy()
    {
        return new AdjustmentSegment(Code, Label, Capacity, Category, Archived, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
