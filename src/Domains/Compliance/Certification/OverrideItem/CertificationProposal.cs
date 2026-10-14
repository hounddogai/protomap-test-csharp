namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification proposal is in its life cycle.</summary>
public enum CertificationProposalStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One certification proposal, with what the planners need to act on it.</summary>
public sealed class CertificationProposal(string code, string name, int quantity, string contactEmail, string location, string checksum, string badge, int durationMinutes, CertificationProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public string ContactEmail { get; set; } = contactEmail;
    public string Location { get; set; } = location;
    public string Checksum { get; set; } = checksum;
    public string Badge { get; set; } = badge;
    public int DurationMinutes { get; set; } = durationMinutes;
    public CertificationProposalStatus Status { get; set; } = status;

    /// <summary>Starts a certification proposal in status new.</summary>
    public static CertificationProposal NewCertificationProposal(string code, string name)
    {
        return new CertificationProposal(code, name, 0, "", "", "", "", 0, CertificationProposalStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationProposalStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationProposalStatus next)
    {
        switch (Status)
        {
            case CertificationProposalStatus.New:
                switch (next)
                {
                    case CertificationProposalStatus.Queued:
                    case CertificationProposalStatus.Done:
                        return true;
                }
                return false;
            case CertificationProposalStatus.Queued:
                return next == CertificationProposalStatus.Done;
            case CertificationProposalStatus.Running:
                return next == CertificationProposalStatus.Done;
            case CertificationProposalStatus.Done:
                return next == CertificationProposalStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CertificationProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)DurationMinutes + (double)Quantity, 2);
    }

    public CertificationProposal Copy()
    {
        return new CertificationProposal(
            Code,
            Name,
            Quantity,
            ContactEmail,
            Location,
            Checksum,
            Badge,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
