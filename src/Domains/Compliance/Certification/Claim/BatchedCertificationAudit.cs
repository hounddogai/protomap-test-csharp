namespace Acme.Workforce.Compliance.Certification.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched certification audit is in its life cycle.</summary>
public enum BatchedCertificationAuditStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One batched certification audit, with what the planners need to act on it.</summary>
public sealed class BatchedCertificationAudit(string code, string name, string segment, double fte, string ownerId, bool visible, BatchedCertificationAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public double Fte { get; set; } = fte;
    public string OwnerId { get; set; } = ownerId;
    public bool Visible { get; set; } = visible;
    public BatchedCertificationAuditStatus Status { get; set; } = status;

    /// <summary>Starts a batched certification audit in status new.</summary>
    public static BatchedCertificationAudit NewBatchedCertificationAudit(string code, string name)
    {
        return new BatchedCertificationAudit(code, name, "", 0.0, "", false, BatchedCertificationAuditStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedCertificationAuditStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedCertificationAuditStatus next)
    {
        switch (Status)
        {
            case BatchedCertificationAuditStatus.New:
                return next == BatchedCertificationAuditStatus.Queued;
            case BatchedCertificationAuditStatus.Queued:
                switch (next)
                {
                    case BatchedCertificationAuditStatus.Failed:
                        return true;
                }
                return false;
            case BatchedCertificationAuditStatus.Running:
                switch (next)
                {
                    case BatchedCertificationAuditStatus.Done:
                        return true;
                }
                return false;
            case BatchedCertificationAuditStatus.Done:
                return next == BatchedCertificationAuditStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BatchedCertificationAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched certification audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public BatchedCertificationAudit Copy()
    {
        return new BatchedCertificationAudit(Code, Name, Segment, Fte, OwnerId, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
