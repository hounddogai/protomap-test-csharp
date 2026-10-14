namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification target is in its life cycle.</summary>
public enum CertificationTargetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A certification target as the certification team keeps it.</summary>
public sealed class CertificationTarget(string code, string label, bool archived, double fte, string summary, double threshold, string skill, string contactEmail, string unit, CertificationTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public double Fte { get; set; } = fte;
    public string Summary { get; set; } = summary;
    public double Threshold { get; set; } = threshold;
    public string Skill { get; set; } = skill;
    public string ContactEmail { get; set; } = contactEmail;
    public string Unit { get; set; } = unit;
    public CertificationTargetStatus Status { get; set; } = status;

    /// <summary>Starts a certification target in status proposed.</summary>
    public static CertificationTarget NewCertificationTarget(string code, string label)
    {
        return new CertificationTarget(
            code,
            label,
            false,
            0.0,
            "",
            0.0,
            "",
            "",
            "",
            CertificationTargetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationTargetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationTargetStatus next)
    {
        switch (Status)
        {
            case CertificationTargetStatus.Proposed:
                return next == CertificationTargetStatus.Withdrawn;
            case CertificationTargetStatus.Approved:
                switch (next)
                {
                    case CertificationTargetStatus.Rejected:
                    case CertificationTargetStatus.Withdrawn:
                        return true;
                }
                return false;
            case CertificationTargetStatus.Rejected:
                switch (next)
                {
                    case CertificationTargetStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetValue()
    {
        return Math.Round(Fte - Threshold, 2);
    }

    public CertificationTarget Copy()
    {
        return new CertificationTarget(
            Code,
            Label,
            Archived,
            Fte,
            Summary,
            Threshold,
            Skill,
            ContactEmail,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
