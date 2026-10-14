namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan snapshot is in its life cycle.</summary>
public enum FloorPlanSnapshotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One floor plan snapshot, with what the planners need to act on it.</summary>
public sealed class FloorPlanSnapshot(string code, string title, double maximum, string source, double score, string externalRef, int attempts, double rate, string contactEmail, FloorPlanSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public string Source { get; set; } = source;
    public double Score { get; set; } = score;
    public string ExternalRef { get; set; } = externalRef;
    public int Attempts { get; set; } = attempts;
    public double Rate { get; set; } = rate;
    public string ContactEmail { get; set; } = contactEmail;
    public FloorPlanSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan snapshot in status open.</summary>
    public static FloorPlanSnapshot NewFloorPlanSnapshot(string code, string title)
    {
        return new FloorPlanSnapshot(code, title, 0.0, "", 0.0, "", 0, 0.0, "", FloorPlanSnapshotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanSnapshotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanSnapshotStatus next)
    {
        switch (Status)
        {
            case FloorPlanSnapshotStatus.Open:
                return next == FloorPlanSnapshotStatus.Resolved || next == FloorPlanSnapshotStatus.Reopened;
            case FloorPlanSnapshotStatus.InReview:
                switch (next)
                {
                    case FloorPlanSnapshotStatus.Resolved:
                    case FloorPlanSnapshotStatus.Reopened:
                        return true;
                }
                return false;
            case FloorPlanSnapshotStatus.Resolved:
                return next == FloorPlanSnapshotStatus.Reopened || next == FloorPlanSnapshotStatus.Archived;
            case FloorPlanSnapshotStatus.Reopened:
                switch (next)
                {
                    case FloorPlanSnapshotStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Attempts - Rate, 2);
    }

    public FloorPlanSnapshot Copy()
    {
        return new FloorPlanSnapshot(
            Code,
            Title,
            Maximum,
            Source,
            Score,
            ExternalRef,
            Attempts,
            Rate,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
