namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict occupancy batch is in its life cycle.</summary>
public enum StrictOccupancyBatchStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One strict occupancy batch, with what the planners need to act on it.</summary>
public sealed class StrictOccupancyBatch(string code, string title, bool pinned, int batchSize, double threshold, int createdAt, string summary, string ownerId, StrictOccupancyBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public int BatchSize { get; set; } = batchSize;
    public double Threshold { get; set; } = threshold;
    public int CreatedAt { get; set; } = createdAt;
    public string Summary { get; set; } = summary;
    public string OwnerId { get; set; } = ownerId;
    public StrictOccupancyBatchStatus Status { get; set; } = status;

    /// <summary>Starts a strict occupancy batch in status proposed.</summary>
    public static StrictOccupancyBatch NewStrictOccupancyBatch(string code, string title)
    {
        return new StrictOccupancyBatch(code, title, false, 0, 0.0, 0, "", "", StrictOccupancyBatchStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictOccupancyBatchStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictOccupancyBatchStatus next)
    {
        switch (Status)
        {
            case StrictOccupancyBatchStatus.Proposed:
                return next == StrictOccupancyBatchStatus.Approved || next == StrictOccupancyBatchStatus.Withdrawn;
            case StrictOccupancyBatchStatus.Approved:
                return next == StrictOccupancyBatchStatus.Rejected || next == StrictOccupancyBatchStatus.Withdrawn;
            case StrictOccupancyBatchStatus.Rejected:
                switch (next)
                {
                    case StrictOccupancyBatchStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictOccupancyBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict occupancy batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)BatchSize + (double)CreatedAt, 2);
    }

    public StrictOccupancyBatch Copy()
    {
        return new StrictOccupancyBatch(
            Code,
            Title,
            Pinned,
            BatchSize,
            Threshold,
            CreatedAt,
            Summary,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
