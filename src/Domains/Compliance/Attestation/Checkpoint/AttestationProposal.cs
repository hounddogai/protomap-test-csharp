namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation proposal is in its life cycle.</summary>
public enum AttestationProposalStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a attestation proposal and its current status.</summary>
public sealed class AttestationProposal(string code, string title, double discount, double weight, int limit, int priority, AttestationProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public double Weight { get; set; } = weight;
    public int Limit { get; set; } = limit;
    public int Priority { get; set; } = priority;
    public AttestationProposalStatus Status { get; set; } = status;

    /// <summary>Starts a attestation proposal in status proposed.</summary>
    public static AttestationProposal NewAttestationProposal(string code, string title)
    {
        return new AttestationProposal(code, title, 0.0, 0.0, 0, 0, AttestationProposalStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationProposalStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationProposalStatus next)
    {
        switch (Status)
        {
            case AttestationProposalStatus.Proposed:
                switch (next)
                {
                    case AttestationProposalStatus.Approved:
                    case AttestationProposalStatus.Withdrawn:
                        return true;
                }
                return false;
            case AttestationProposalStatus.Approved:
                switch (next)
                {
                    case AttestationProposalStatus.Withdrawn:
                        return true;
                }
                return false;
            case AttestationProposalStatus.Rejected:
                switch (next)
                {
                    case AttestationProposalStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"attestation proposal"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Discount * (double)Priority, 2);
    }

    public AttestationProposal Copy()
    {
        return new AttestationProposal(Code, Title, Discount, Weight, Limit, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
