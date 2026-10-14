namespace Acme.Workforce.Compliance.TrainingDue.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due audit is in its life cycle.</summary>
public enum TrainingDueAuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A training due audit as the training due team keeps it.</summary>
public sealed class TrainingDueAudit(string code, string title, string summary, int createdAt, string badge, int batchSize, double amount, TrainingDueAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public int CreatedAt { get; set; } = createdAt;
    public string Badge { get; set; } = badge;
    public int BatchSize { get; set; } = batchSize;
    public double Amount { get; set; } = amount;
    public TrainingDueAuditStatus Status { get; set; } = status;

    /// <summary>Starts a training due audit in status planned.</summary>
    public static TrainingDueAudit NewTrainingDueAudit(string code, string title)
    {
        return new TrainingDueAudit(code, title, "", 0, "", 0, 0.0, TrainingDueAuditStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueAuditStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueAuditStatus next)
    {
        switch (Status)
        {
            case TrainingDueAuditStatus.Planned:
                return next == TrainingDueAuditStatus.Cancelled;
            case TrainingDueAuditStatus.Scheduled:
                switch (next)
                {
                    case TrainingDueAuditStatus.Cancelled:
                        return true;
                }
                return false;
            case TrainingDueAuditStatus.InProgress:
                switch (next)
                {
                    case TrainingDueAuditStatus.Completed:
                        return true;
                }
                return false;
            case TrainingDueAuditStatus.Completed:
                return next == TrainingDueAuditStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TrainingDueAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"training due audit"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)BatchSize + Amount, 2);
    }

    public TrainingDueAudit Copy()
    {
        return new TrainingDueAudit(Code, Title, Summary, CreatedAt, Badge, BatchSize, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
