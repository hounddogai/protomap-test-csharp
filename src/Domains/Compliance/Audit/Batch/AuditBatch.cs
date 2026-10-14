namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit batch is in its life cycle.</summary>
public enum AuditBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a audit batch and its current status.</summary>
public sealed class AuditBatch(string code, string title, double discount, int retries, string badge, int createdAt, AuditBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public int Retries { get; set; } = retries;
    public string Badge { get; set; } = badge;
    public int CreatedAt { get; set; } = createdAt;
    public AuditBatchStatus Status { get; set; } = status;

    /// <summary>Starts a audit batch in status open.</summary>
    public static AuditBatch NewAuditBatch(string code, string title)
    {
        return new AuditBatch(code, title, 0.0, 0, "", 0, AuditBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditBatchStatus next)
    {
        switch (Status)
        {
            case AuditBatchStatus.Open:
                return next == AuditBatchStatus.InReview || next == AuditBatchStatus.Resolved;
            case AuditBatchStatus.InReview:
                switch (next)
                {
                    case AuditBatchStatus.Archived:
                        return true;
                }
                return false;
            case AuditBatchStatus.Resolved:
                return next == AuditBatchStatus.Reopened;
            case AuditBatchStatus.Reopened:
                switch (next)
                {
                    case AuditBatchStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)CreatedAt * Discount, 2);
    }

    public AuditBatch Copy()
    {
        return new AuditBatch(Code, Title, Discount, Retries, Badge, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
