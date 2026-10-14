namespace Acme.Workforce.Engagement.TownHall.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched town hall request is in its life cycle.</summary>
public enum BatchedTownHallRequestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One batched town hall request, with what the planners need to act on it.</summary>
public sealed class BatchedTownHallRequest(string code, string title, int startsAt, string managerId, int batchSize, string timezone, int sequence, string reviewer, int durationMinutes, int capacity, BatchedTownHallRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public string ManagerId { get; set; } = managerId;
    public int BatchSize { get; set; } = batchSize;
    public string Timezone { get; set; } = timezone;
    public int Sequence { get; set; } = sequence;
    public string Reviewer { get; set; } = reviewer;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Capacity { get; set; } = capacity;
    public BatchedTownHallRequestStatus Status { get; set; } = status;

    /// <summary>Starts a batched town hall request in status created.</summary>
    public static BatchedTownHallRequest NewBatchedTownHallRequest(string code, string title)
    {
        return new BatchedTownHallRequest(
            code,
            title,
            0,
            "",
            0,
            "",
            0,
            "",
            0,
            0,
            BatchedTownHallRequestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTownHallRequestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTownHallRequestStatus next)
    {
        switch (Status)
        {
            case BatchedTownHallRequestStatus.Created:
                switch (next)
                {
                    case BatchedTownHallRequestStatus.Validated:
                    case BatchedTownHallRequestStatus.Published:
                        return true;
                }
                return false;
            case BatchedTownHallRequestStatus.Validated:
                switch (next)
                {
                    case BatchedTownHallRequestStatus.Retired:
                        return true;
                }
                return false;
            case BatchedTownHallRequestStatus.Published:
                return next == BatchedTownHallRequestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BatchedTownHallRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched town hall request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)DurationMinutes + (double)BatchSize, 2);
    }

    public BatchedTownHallRequest Copy()
    {
        return new BatchedTownHallRequest(
            Code,
            Title,
            StartsAt,
            ManagerId,
            BatchSize,
            Timezone,
            Sequence,
            Reviewer,
            DurationMinutes,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
