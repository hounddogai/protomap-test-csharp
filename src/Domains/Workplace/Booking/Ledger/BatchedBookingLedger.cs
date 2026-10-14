namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched booking ledger is in its life cycle.</summary>
public enum BatchedBookingLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A batched booking ledger. Its code is unique within a tenant.</summary>
public sealed class BatchedBookingLedger(string code, string label, string segment, string location, bool pinned, int priority, string name, string jobCode, string notes, int limit, double minimum, BatchedBookingLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string Location { get; set; } = location;
    public bool Pinned { get; set; } = pinned;
    public int Priority { get; set; } = priority;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string Notes { get; set; } = notes;
    public int Limit { get; set; } = limit;
    public double Minimum { get; set; } = minimum;
    public BatchedBookingLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a batched booking ledger in status planned.</summary>
    public static BatchedBookingLedger NewBatchedBookingLedger(string code, string label)
    {
        return new BatchedBookingLedger(
            code,
            label,
            "",
            "",
            false,
            0,
            "",
            "",
            "",
            0,
            0.0,
            BatchedBookingLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedBookingLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedBookingLedgerStatus next)
    {
        switch (Status)
        {
            case BatchedBookingLedgerStatus.Planned:
                switch (next)
                {
                    case BatchedBookingLedgerStatus.InProgress:
                    case BatchedBookingLedgerStatus.Completed:
                        return true;
                }
                return false;
            case BatchedBookingLedgerStatus.Scheduled:
                return next == BatchedBookingLedgerStatus.InProgress || next == BatchedBookingLedgerStatus.Cancelled;
            case BatchedBookingLedgerStatus.InProgress:
                switch (next)
                {
                    case BatchedBookingLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case BatchedBookingLedgerStatus.Completed:
                return next == BatchedBookingLedgerStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BatchedBookingLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched booking ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"batched booking ledger"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Minimum - (double)Priority, 2);
    }

    public BatchedBookingLedger Copy()
    {
        return new BatchedBookingLedger(
            Code,
            Label,
            Segment,
            Location,
            Pinned,
            Priority,
            Name,
            JobCode,
            Notes,
            Limit,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
