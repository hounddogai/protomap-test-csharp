namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending leave request batch is in its life cycle.</summary>
public enum PendingLeaveRequestBatchStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pending leave request batch. Its code is unique within a tenant.</summary>
public sealed class PendingLeaveRequestBatch(string code, string title, string segment, double floorValue, int endsAt, bool visible, PendingLeaveRequestBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public double FloorValue { get; set; } = floorValue;
    public int EndsAt { get; set; } = endsAt;
    public bool Visible { get; set; } = visible;
    public PendingLeaveRequestBatchStatus Status { get; set; } = status;

    /// <summary>Starts a pending leave request batch in status proposed.</summary>
    public static PendingLeaveRequestBatch NewPendingLeaveRequestBatch(string code, string title)
    {
        return new PendingLeaveRequestBatch(code, title, "", 0.0, 0, false, PendingLeaveRequestBatchStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingLeaveRequestBatchStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingLeaveRequestBatchStatus next)
    {
        switch (Status)
        {
            case PendingLeaveRequestBatchStatus.Proposed:
                switch (next)
                {
                    case PendingLeaveRequestBatchStatus.Approved:
                    case PendingLeaveRequestBatchStatus.Withdrawn:
                        return true;
                }
                return false;
            case PendingLeaveRequestBatchStatus.Approved:
                return next == PendingLeaveRequestBatchStatus.Rejected || next == PendingLeaveRequestBatchStatus.Withdrawn;
            case PendingLeaveRequestBatchStatus.Rejected:
                switch (next)
                {
                    case PendingLeaveRequestBatchStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingLeaveRequestBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending leave request batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)EndsAt + FloorValue, 2);
    }

    public PendingLeaveRequestBatch Copy()
    {
        return new PendingLeaveRequestBatch(Code, Title, Segment, FloorValue, EndsAt, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
