namespace Acme.Workforce.Compliance.TrainingDue.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered training due audit is in its life cycle.</summary>
public enum TieredTrainingDueAuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tiered training due audit as the training due team keeps it.</summary>
public sealed class TieredTrainingDueAudit(string code, string name, double maximum, int priority, string channel, int createdAt, double threshold, string contactEmail, TieredTrainingDueAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public int Priority { get; set; } = priority;
    public string Channel { get; set; } = channel;
    public int CreatedAt { get; set; } = createdAt;
    public double Threshold { get; set; } = threshold;
    public string ContactEmail { get; set; } = contactEmail;
    public TieredTrainingDueAuditStatus Status { get; set; } = status;

    /// <summary>Starts a tiered training due audit in status planned.</summary>
    public static TieredTrainingDueAudit NewTieredTrainingDueAudit(string code, string name)
    {
        return new TieredTrainingDueAudit(code, name, 0.0, 0, "", 0, 0.0, "", TieredTrainingDueAuditStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredTrainingDueAuditStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredTrainingDueAuditStatus next)
    {
        switch (Status)
        {
            case TieredTrainingDueAuditStatus.Planned:
                switch (next)
                {
                    case TieredTrainingDueAuditStatus.Scheduled:
                    case TieredTrainingDueAuditStatus.InProgress:
                        return true;
                }
                return false;
            case TieredTrainingDueAuditStatus.Scheduled:
                switch (next)
                {
                    case TieredTrainingDueAuditStatus.Completed:
                    case TieredTrainingDueAuditStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredTrainingDueAuditStatus.InProgress:
                return next == TieredTrainingDueAuditStatus.Cancelled;
            case TieredTrainingDueAuditStatus.Completed:
                switch (next)
                {
                    case TieredTrainingDueAuditStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredTrainingDueAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered training due audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetValue()
    {
        return Math.Round((double)Priority + (double)CreatedAt, 2);
    }

    public TieredTrainingDueAudit Copy()
    {
        return new TieredTrainingDueAudit(
            Code,
            Name,
            Maximum,
            Priority,
            Channel,
            CreatedAt,
            Threshold,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
