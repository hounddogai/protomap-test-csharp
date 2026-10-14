namespace Acme.Workforce.Talent.Mobility.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched mobility slot is in its life cycle.</summary>
public enum BatchedMobilitySlotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A batched mobility slot as the mobility team keeps it.</summary>
public sealed class BatchedMobilitySlot(string code, string title, double floorValue, int expiresAt, int quantity, BatchedMobilitySlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Quantity { get; set; } = quantity;
    public BatchedMobilitySlotStatus Status { get; set; } = status;

    /// <summary>Starts a batched mobility slot in status draft.</summary>
    public static BatchedMobilitySlot NewBatchedMobilitySlot(string code, string title)
    {
        return new BatchedMobilitySlot(code, title, 0.0, 0, 0, BatchedMobilitySlotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedMobilitySlotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedMobilitySlotStatus next)
    {
        switch (Status)
        {
            case BatchedMobilitySlotStatus.Draft:
                return next == BatchedMobilitySlotStatus.Active;
            case BatchedMobilitySlotStatus.Pending:
                switch (next)
                {
                    case BatchedMobilitySlotStatus.Suspended:
                    case BatchedMobilitySlotStatus.Closed:
                        return true;
                }
                return false;
            case BatchedMobilitySlotStatus.Active:
                switch (next)
                {
                    case BatchedMobilitySlotStatus.Suspended:
                    case BatchedMobilitySlotStatus.Closed:
                        return true;
                }
                return false;
            case BatchedMobilitySlotStatus.Suspended:
                switch (next)
                {
                    case BatchedMobilitySlotStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedMobilitySlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched mobility slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"batched mobility slot"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)ExpiresAt * FloorValue, 2);
    }

    public BatchedMobilitySlot Copy()
    {
        return new BatchedMobilitySlot(Code, Title, FloorValue, ExpiresAt, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
