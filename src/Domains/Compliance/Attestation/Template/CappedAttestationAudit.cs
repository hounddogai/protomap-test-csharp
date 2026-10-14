namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped attestation audit is in its life cycle.</summary>
public enum CappedAttestationAuditStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A capped attestation audit. Its code is unique within a tenant.</summary>
public sealed class CappedAttestationAudit(string code, string label, int retries, int leaveDays, string managerId, double threshold, List<string> tags, string description, double maximum, double ceiling, int windowDays, CappedAttestationAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public int LeaveDays { get; set; } = leaveDays;
    public string ManagerId { get; set; } = managerId;
    public double Threshold { get; set; } = threshold;
    public List<string> Tags { get; set; } = tags;
    public string Description { get; set; } = description;
    public double Maximum { get; set; } = maximum;
    public double Ceiling { get; set; } = ceiling;
    public int WindowDays { get; set; } = windowDays;
    public CappedAttestationAuditStatus Status { get; set; } = status;

    /// <summary>Starts a capped attestation audit in status new.</summary>
    public static CappedAttestationAudit NewCappedAttestationAudit(string code, string label)
    {
        return new CappedAttestationAudit(
            code,
            label,
            0,
            0,
            "",
            0.0,
            new List<string>(),
            "",
            0.0,
            0.0,
            0,
            CappedAttestationAuditStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAttestationAuditStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAttestationAuditStatus next)
    {
        switch (Status)
        {
            case CappedAttestationAuditStatus.New:
                return next == CappedAttestationAuditStatus.Done;
            case CappedAttestationAuditStatus.Queued:
                return next == CappedAttestationAuditStatus.Running;
            case CappedAttestationAuditStatus.Running:
                switch (next)
                {
                    case CappedAttestationAuditStatus.Done:
                    case CappedAttestationAuditStatus.Failed:
                        return true;
                }
                return false;
            case CappedAttestationAuditStatus.Done:
                switch (next)
                {
                    case CappedAttestationAuditStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedAttestationAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped attestation audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"capped attestation audit"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedValue()
    {
        return Math.Round((double)LeaveDays - (double)Retries, 2);
    }

    public CappedAttestationAudit Copy()
    {
        return new CappedAttestationAudit(
            Code,
            Label,
            Retries,
            LeaveDays,
            ManagerId,
            Threshold,
            Tags,
            Description,
            Maximum,
            Ceiling,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
