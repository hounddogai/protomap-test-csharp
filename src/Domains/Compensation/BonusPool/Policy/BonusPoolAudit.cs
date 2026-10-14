namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool audit is in its life cycle.</summary>
public enum BonusPoolAuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A bonus pool audit. Its code is unique within a tenant.</summary>
public sealed class BonusPoolAudit(string code, string name, int quantity, string contactEmail, bool archived, int durationMinutes, int limit, int createdAt, string costCenter, BonusPoolAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Archived { get; set; } = archived;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Limit { get; set; } = limit;
    public int CreatedAt { get; set; } = createdAt;
    public string CostCenter { get; set; } = costCenter;
    public BonusPoolAuditStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool audit in status planned.</summary>
    public static BonusPoolAudit NewBonusPoolAudit(string code, string name)
    {
        return new BonusPoolAudit(code, name, 0, "", false, 0, 0, 0, "", BonusPoolAuditStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolAuditStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolAuditStatus next)
    {
        switch (Status)
        {
            case BonusPoolAuditStatus.Planned:
                return next == BonusPoolAuditStatus.Cancelled;
            case BonusPoolAuditStatus.Scheduled:
                return next == BonusPoolAuditStatus.Completed;
            case BonusPoolAuditStatus.InProgress:
                switch (next)
                {
                    case BonusPoolAuditStatus.Completed:
                    case BonusPoolAuditStatus.Cancelled:
                        return true;
                }
                return false;
            case BonusPoolAuditStatus.Completed:
                switch (next)
                {
                    case BonusPoolAuditStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Quantity * (double)Limit, 2);
    }

    public BonusPoolAudit Copy()
    {
        return new BonusPoolAudit(
            Code,
            Name,
            Quantity,
            ContactEmail,
            Archived,
            DurationMinutes,
            Limit,
            CreatedAt,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
