namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped visitor batch is in its life cycle.</summary>
public enum CappedVisitorBatchStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A capped visitor batch as the visitor team keeps it.</summary>
public sealed class CappedVisitorBatch(string code, string title, string externalRef, bool active, string phone, string category, int startsAt, int updatedAt, int leaveDays, CappedVisitorBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public bool Active { get; set; } = active;
    public string Phone { get; set; } = phone;
    public string Category { get; set; } = category;
    public int StartsAt { get; set; } = startsAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public int LeaveDays { get; set; } = leaveDays;
    public CappedVisitorBatchStatus Status { get; set; } = status;

    /// <summary>Starts a capped visitor batch in status requested.</summary>
    public static CappedVisitorBatch NewCappedVisitorBatch(string code, string title)
    {
        return new CappedVisitorBatch(code, title, "", false, "", "", 0, 0, 0, CappedVisitorBatchStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedVisitorBatchStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedVisitorBatchStatus next)
    {
        switch (Status)
        {
            case CappedVisitorBatchStatus.Requested:
                return next == CappedVisitorBatchStatus.Granted;
            case CappedVisitorBatchStatus.Granted:
                switch (next)
                {
                    case CappedVisitorBatchStatus.Revoked:
                    case CappedVisitorBatchStatus.Expired:
                        return true;
                }
                return false;
            case CappedVisitorBatchStatus.Revoked:
                switch (next)
                {
                    case CappedVisitorBatchStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedVisitorBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped visitor batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)LeaveDays + (double)UpdatedAt, 2);
    }

    public CappedVisitorBatch Copy()
    {
        return new CappedVisitorBatch(
            Code,
            Title,
            ExternalRef,
            Active,
            Phone,
            Category,
            StartsAt,
            UpdatedAt,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
