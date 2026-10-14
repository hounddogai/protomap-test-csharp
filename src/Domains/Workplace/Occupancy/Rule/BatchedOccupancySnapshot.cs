namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched occupancy snapshot is in its life cycle.</summary>
public enum BatchedOccupancySnapshotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One batched occupancy snapshot, with what the planners need to act on it.</summary>
public sealed class BatchedOccupancySnapshot(string code, string title, List<string> tags, int priority, int leaveDays, string salaryBand, double rating, bool approved, string grade, BatchedOccupancySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public int Priority { get; set; } = priority;
    public int LeaveDays { get; set; } = leaveDays;
    public string SalaryBand { get; set; } = salaryBand;
    public double Rating { get; set; } = rating;
    public bool Approved { get; set; } = approved;
    public string Grade { get; set; } = grade;
    public BatchedOccupancySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a batched occupancy snapshot in status open.</summary>
    public static BatchedOccupancySnapshot NewBatchedOccupancySnapshot(string code, string title)
    {
        return new BatchedOccupancySnapshot(
            code,
            title,
            new List<string>(),
            0,
            0,
            "",
            0.0,
            false,
            "",
            BatchedOccupancySnapshotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedOccupancySnapshotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedOccupancySnapshotStatus next)
    {
        switch (Status)
        {
            case BatchedOccupancySnapshotStatus.Open:
                return next == BatchedOccupancySnapshotStatus.InReview || next == BatchedOccupancySnapshotStatus.Resolved;
            case BatchedOccupancySnapshotStatus.InReview:
                switch (next)
                {
                    case BatchedOccupancySnapshotStatus.Resolved:
                    case BatchedOccupancySnapshotStatus.Archived:
                        return true;
                }
                return false;
            case BatchedOccupancySnapshotStatus.Resolved:
                return next == BatchedOccupancySnapshotStatus.Reopened;
            case BatchedOccupancySnapshotStatus.Reopened:
                return next == BatchedOccupancySnapshotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BatchedOccupancySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched occupancy snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)LeaveDays + Rating, 2);
    }

    public BatchedOccupancySnapshot Copy()
    {
        return new BatchedOccupancySnapshot(
            Code,
            Title,
            Tags,
            Priority,
            LeaveDays,
            SalaryBand,
            Rating,
            Approved,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
