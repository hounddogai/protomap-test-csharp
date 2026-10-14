namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched recognition slot is in its life cycle.</summary>
public enum BatchedRecognitionSlotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A batched recognition slot. Its code is unique within a tenant.</summary>
public sealed class BatchedRecognitionSlot(string code, string name, string unit, bool pinned, int createdAt, double discount, bool archived, string ownerId, BatchedRecognitionSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public bool Pinned { get; set; } = pinned;
    public int CreatedAt { get; set; } = createdAt;
    public double Discount { get; set; } = discount;
    public bool Archived { get; set; } = archived;
    public string OwnerId { get; set; } = ownerId;
    public BatchedRecognitionSlotStatus Status { get; set; } = status;

    /// <summary>Starts a batched recognition slot in status draft.</summary>
    public static BatchedRecognitionSlot NewBatchedRecognitionSlot(string code, string name)
    {
        return new BatchedRecognitionSlot(
            code,
            name,
            "",
            false,
            0,
            0.0,
            false,
            "",
            BatchedRecognitionSlotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedRecognitionSlotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedRecognitionSlotStatus next)
    {
        switch (Status)
        {
            case BatchedRecognitionSlotStatus.Draft:
                return next == BatchedRecognitionSlotStatus.Pending || next == BatchedRecognitionSlotStatus.Closed;
            case BatchedRecognitionSlotStatus.Pending:
                return next == BatchedRecognitionSlotStatus.Suspended || next == BatchedRecognitionSlotStatus.Closed;
            case BatchedRecognitionSlotStatus.Active:
                switch (next)
                {
                    case BatchedRecognitionSlotStatus.Suspended:
                    case BatchedRecognitionSlotStatus.Closed:
                        return true;
                }
                return false;
            case BatchedRecognitionSlotStatus.Suspended:
                switch (next)
                {
                    case BatchedRecognitionSlotStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedRecognitionSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched recognition slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"batched recognition slot"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)CreatedAt - Discount, 2);
    }

    public BatchedRecognitionSlot Copy()
    {
        return new BatchedRecognitionSlot(Code, Name, Unit, Pinned, CreatedAt, Discount, Archived, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
