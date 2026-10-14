namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched booking audit is in its life cycle.</summary>
public enum BatchedBookingAuditStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A batched booking audit. Its code is unique within a tenant.</summary>
public sealed class BatchedBookingAudit(string code, string title, int windowDays, bool pinned, string description, BatchedBookingAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public bool Pinned { get; set; } = pinned;
    public string Description { get; set; } = description;
    public BatchedBookingAuditStatus Status { get; set; } = status;

    /// <summary>Starts a batched booking audit in status new.</summary>
    public static BatchedBookingAudit NewBatchedBookingAudit(string code, string title)
    {
        return new BatchedBookingAudit(code, title, 0, false, "", BatchedBookingAuditStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedBookingAuditStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedBookingAuditStatus next)
    {
        switch (Status)
        {
            case BatchedBookingAuditStatus.New:
                return next == BatchedBookingAuditStatus.Failed;
            case BatchedBookingAuditStatus.Queued:
                return next == BatchedBookingAuditStatus.Done || next == BatchedBookingAuditStatus.Failed;
            case BatchedBookingAuditStatus.Running:
                switch (next)
                {
                    case BatchedBookingAuditStatus.Done:
                    case BatchedBookingAuditStatus.Failed:
                        return true;
                }
                return false;
            case BatchedBookingAuditStatus.Done:
                return next == BatchedBookingAuditStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BatchedBookingAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched booking audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public BatchedBookingAudit Copy()
    {
        return new BatchedBookingAudit(Code, Title, WindowDays, Pinned, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
