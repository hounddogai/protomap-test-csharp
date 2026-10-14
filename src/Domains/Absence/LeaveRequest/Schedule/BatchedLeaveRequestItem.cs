namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched leave request item is in its life cycle.</summary>
public enum BatchedLeaveRequestItemStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One batched leave request item, with what the planners need to act on it.</summary>
public sealed class BatchedLeaveRequestItem(string code, string title, int batchSize, double fte, string timezone, bool active, int updatedAt, string label, double ceiling, string category, int quantity, BatchedLeaveRequestItemStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public double Fte { get; set; } = fte;
    public string Timezone { get; set; } = timezone;
    public bool Active { get; set; } = active;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public string Category { get; set; } = category;
    public int Quantity { get; set; } = quantity;
    public BatchedLeaveRequestItemStatus Status { get; set; } = status;

    /// <summary>Starts a batched leave request item in status requested.</summary>
    public static BatchedLeaveRequestItem NewBatchedLeaveRequestItem(string code, string title)
    {
        return new BatchedLeaveRequestItem(
            code,
            title,
            0,
            0.0,
            "",
            false,
            0,
            "",
            0.0,
            "",
            0,
            BatchedLeaveRequestItemStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedLeaveRequestItemStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedLeaveRequestItemStatus next)
    {
        switch (Status)
        {
            case BatchedLeaveRequestItemStatus.Requested:
                switch (next)
                {
                    case BatchedLeaveRequestItemStatus.Granted:
                    case BatchedLeaveRequestItemStatus.Revoked:
                        return true;
                }
                return false;
            case BatchedLeaveRequestItemStatus.Granted:
                return next == BatchedLeaveRequestItemStatus.Expired;
            case BatchedLeaveRequestItemStatus.Revoked:
                switch (next)
                {
                    case BatchedLeaveRequestItemStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedLeaveRequestItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched leave request item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Ceiling - (double)BatchSize, 2);
    }

    public BatchedLeaveRequestItem Copy()
    {
        return new BatchedLeaveRequestItem(
            Code,
            Title,
            BatchSize,
            Fte,
            Timezone,
            Active,
            UpdatedAt,
            Label,
            Ceiling,
            Category,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
