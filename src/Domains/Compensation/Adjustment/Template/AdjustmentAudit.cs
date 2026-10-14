namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment audit is in its life cycle.</summary>
public enum AdjustmentAuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A adjustment audit. Its code is unique within a tenant.</summary>
public sealed class AdjustmentAudit(string code, string label, string externalRef, int quantity, int createdAt, AdjustmentAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public int Quantity { get; set; } = quantity;
    public int CreatedAt { get; set; } = createdAt;
    public AdjustmentAuditStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment audit in status planned.</summary>
    public static AdjustmentAudit NewAdjustmentAudit(string code, string label)
    {
        return new AdjustmentAudit(code, label, "", 0, 0, AdjustmentAuditStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentAuditStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentAuditStatus next)
    {
        switch (Status)
        {
            case AdjustmentAuditStatus.Planned:
                return next == AdjustmentAuditStatus.Scheduled || next == AdjustmentAuditStatus.Cancelled;
            case AdjustmentAuditStatus.Scheduled:
                return next == AdjustmentAuditStatus.Cancelled;
            case AdjustmentAuditStatus.InProgress:
                switch (next)
                {
                    case AdjustmentAuditStatus.Completed:
                    case AdjustmentAuditStatus.Cancelled:
                        return true;
                }
                return false;
            case AdjustmentAuditStatus.Completed:
                switch (next)
                {
                    case AdjustmentAuditStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Quantity - (double)CreatedAt, 2);
    }

    public AdjustmentAudit Copy()
    {
        return new AdjustmentAudit(Code, Label, ExternalRef, Quantity, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
