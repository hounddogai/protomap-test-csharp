namespace Acme.Workforce.Compliance.Certification.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive certification proposal is in its life cycle.</summary>
public enum AdaptiveCertificationProposalStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A adaptive certification proposal. Its code is unique within a tenant.</summary>
public sealed class AdaptiveCertificationProposal(string code, string label, string title, double cost, string managerId, int sequence, string description, AdaptiveCertificationProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public string ManagerId { get; set; } = managerId;
    public int Sequence { get; set; } = sequence;
    public string Description { get; set; } = description;
    public AdaptiveCertificationProposalStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive certification proposal in status created.</summary>
    public static AdaptiveCertificationProposal NewAdaptiveCertificationProposal(string code, string label)
    {
        return new AdaptiveCertificationProposal(
            code,
            label,
            "",
            0.0,
            "",
            0,
            "",
            AdaptiveCertificationProposalStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveCertificationProposalStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveCertificationProposalStatus next)
    {
        switch (Status)
        {
            case AdaptiveCertificationProposalStatus.Created:
                switch (next)
                {
                    case AdaptiveCertificationProposalStatus.Validated:
                    case AdaptiveCertificationProposalStatus.Retired:
                        return true;
                }
                return false;
            case AdaptiveCertificationProposalStatus.Validated:
                return next == AdaptiveCertificationProposalStatus.Published || next == AdaptiveCertificationProposalStatus.Retired;
            case AdaptiveCertificationProposalStatus.Published:
                return next == AdaptiveCertificationProposalStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdaptiveCertificationProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive certification proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Cost - (double)Sequence, 2);
    }

    public AdaptiveCertificationProposal Copy()
    {
        return new AdaptiveCertificationProposal(Code, Label, Title, Cost, ManagerId, Sequence, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
