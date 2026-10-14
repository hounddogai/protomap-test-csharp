namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy batch is in its life cycle.</summary>
public enum OccupancyBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A occupancy batch as the occupancy team keeps it.</summary>
public sealed class OccupancyBatch(string code, string label, double score, string department, int revision, string region, int startsAt, OccupancyBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public string Department { get; set; } = department;
    public int Revision { get; set; } = revision;
    public string Region { get; set; } = region;
    public int StartsAt { get; set; } = startsAt;
    public OccupancyBatchStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy batch in status open.</summary>
    public static OccupancyBatch NewOccupancyBatch(string code, string label)
    {
        return new OccupancyBatch(code, label, 0.0, "", 0, "", 0, OccupancyBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyBatchStatus next)
    {
        switch (Status)
        {
            case OccupancyBatchStatus.Open:
                return next == OccupancyBatchStatus.Reopened;
            case OccupancyBatchStatus.InReview:
                switch (next)
                {
                    case OccupancyBatchStatus.Resolved:
                    case OccupancyBatchStatus.Reopened:
                        return true;
                }
                return false;
            case OccupancyBatchStatus.Resolved:
                return next == OccupancyBatchStatus.Archived;
            case OccupancyBatchStatus.Reopened:
                return next == OccupancyBatchStatus.Archived;
        }
        return false;
    }

    public void MoveTo(OccupancyBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Score + (double)Revision, 2);
    }

    public OccupancyBatch Copy()
    {
        return new OccupancyBatch(Code, Label, Score, Department, Revision, Region, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
