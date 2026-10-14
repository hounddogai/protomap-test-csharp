namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped attestation proposal is in its life cycle.</summary>
public enum CappedAttestationProposalStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a capped attestation proposal and its current status.</summary>
public sealed class CappedAttestationProposal(string code, string title, int capacity, string locale, double discount, double margin, CappedAttestationProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public string Locale { get; set; } = locale;
    public double Discount { get; set; } = discount;
    public double Margin { get; set; } = margin;
    public CappedAttestationProposalStatus Status { get; set; } = status;

    /// <summary>Starts a capped attestation proposal in status open.</summary>
    public static CappedAttestationProposal NewCappedAttestationProposal(string code, string title)
    {
        return new CappedAttestationProposal(code, title, 0, "", 0.0, 0.0, CappedAttestationProposalStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAttestationProposalStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAttestationProposalStatus next)
    {
        switch (Status)
        {
            case CappedAttestationProposalStatus.Open:
                return next == CappedAttestationProposalStatus.Reopened || next == CappedAttestationProposalStatus.Archived;
            case CappedAttestationProposalStatus.InReview:
                switch (next)
                {
                    case CappedAttestationProposalStatus.Resolved:
                    case CappedAttestationProposalStatus.Archived:
                        return true;
                }
                return false;
            case CappedAttestationProposalStatus.Resolved:
                switch (next)
                {
                    case CappedAttestationProposalStatus.Reopened:
                    case CappedAttestationProposalStatus.Archived:
                        return true;
                }
                return false;
            case CappedAttestationProposalStatus.Reopened:
                switch (next)
                {
                    case CappedAttestationProposalStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedAttestationProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped attestation proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped attestation proposal"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Margin * Discount, 2);
    }

    public CappedAttestationProposal Copy()
    {
        return new CappedAttestationProposal(Code, Title, Capacity, Locale, Discount, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
