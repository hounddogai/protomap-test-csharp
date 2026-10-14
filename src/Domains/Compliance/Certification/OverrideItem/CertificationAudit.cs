namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification audit is in its life cycle.</summary>
public enum CertificationAuditStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A certification audit as the certification team keeps it.</summary>
public sealed class CertificationAudit(string code, string label, int version, string checksum, int startsAt, CertificationAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public string Checksum { get; set; } = checksum;
    public int StartsAt { get; set; } = startsAt;
    public CertificationAuditStatus Status { get; set; } = status;

    /// <summary>Starts a certification audit in status proposed.</summary>
    public static CertificationAudit NewCertificationAudit(string code, string label)
    {
        return new CertificationAudit(code, label, 0, "", 0, CertificationAuditStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationAuditStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationAuditStatus next)
    {
        switch (Status)
        {
            case CertificationAuditStatus.Proposed:
                switch (next)
                {
                    case CertificationAuditStatus.Withdrawn:
                        return true;
                }
                return false;
            case CertificationAuditStatus.Approved:
                switch (next)
                {
                    case CertificationAuditStatus.Rejected:
                        return true;
                }
                return false;
            case CertificationAuditStatus.Rejected:
                return next == CertificationAuditStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CertificationAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)StartsAt * (double)Version, 2);
    }

    public CertificationAudit Copy()
    {
        return new CertificationAudit(Code, Label, Version, Checksum, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
