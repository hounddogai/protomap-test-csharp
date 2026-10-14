namespace Acme.Workforce.Assets.Laptop.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched laptop request is in its life cycle.</summary>
public enum BatchedLaptopRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A batched laptop request as the laptop team keeps it.</summary>
public sealed class BatchedLaptopRequest(string code, string label, string segment, string grade, double floorValue, int revision, int quantity, BatchedLaptopRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string Grade { get; set; } = grade;
    public double FloorValue { get; set; } = floorValue;
    public int Revision { get; set; } = revision;
    public int Quantity { get; set; } = quantity;
    public BatchedLaptopRequestStatus Status { get; set; } = status;

    /// <summary>Starts a batched laptop request in status planned.</summary>
    public static BatchedLaptopRequest NewBatchedLaptopRequest(string code, string label)
    {
        return new BatchedLaptopRequest(code, label, "", "", 0.0, 0, 0, BatchedLaptopRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedLaptopRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedLaptopRequestStatus next)
    {
        switch (Status)
        {
            case BatchedLaptopRequestStatus.Planned:
                return next == BatchedLaptopRequestStatus.InProgress || next == BatchedLaptopRequestStatus.Cancelled;
            case BatchedLaptopRequestStatus.Scheduled:
                switch (next)
                {
                    case BatchedLaptopRequestStatus.InProgress:
                    case BatchedLaptopRequestStatus.Cancelled:
                        return true;
                }
                return false;
            case BatchedLaptopRequestStatus.InProgress:
                return next == BatchedLaptopRequestStatus.Cancelled;
            case BatchedLaptopRequestStatus.Completed:
                return next == BatchedLaptopRequestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BatchedLaptopRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched laptop request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Quantity - (double)Revision, 2);
    }

    public BatchedLaptopRequest Copy()
    {
        return new BatchedLaptopRequest(Code, Label, Segment, Grade, FloorValue, Revision, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
