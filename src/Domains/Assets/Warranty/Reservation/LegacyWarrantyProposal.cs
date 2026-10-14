namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy warranty proposal is in its life cycle.</summary>
public enum LegacyWarrantyProposalStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One legacy warranty proposal, with what the planners need to act on it.</summary>
public sealed class LegacyWarrantyProposal(string code, string title, string description, string team, int capacity, string currency, double ratio, string name, LegacyWarrantyProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public string Team { get; set; } = team;
    public int Capacity { get; set; } = capacity;
    public string Currency { get; set; } = currency;
    public double Ratio { get; set; } = ratio;
    public string Name { get; set; } = name;
    public LegacyWarrantyProposalStatus Status { get; set; } = status;

    /// <summary>Starts a legacy warranty proposal in status requested.</summary>
    public static LegacyWarrantyProposal NewLegacyWarrantyProposal(string code, string title)
    {
        return new LegacyWarrantyProposal(
            code,
            title,
            "",
            "",
            0,
            "",
            0.0,
            "",
            LegacyWarrantyProposalStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyWarrantyProposalStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyWarrantyProposalStatus next)
    {
        switch (Status)
        {
            case LegacyWarrantyProposalStatus.Requested:
                switch (next)
                {
                    case LegacyWarrantyProposalStatus.Granted:
                    case LegacyWarrantyProposalStatus.Revoked:
                        return true;
                }
                return false;
            case LegacyWarrantyProposalStatus.Granted:
                switch (next)
                {
                    case LegacyWarrantyProposalStatus.Expired:
                        return true;
                }
                return false;
            case LegacyWarrantyProposalStatus.Revoked:
                switch (next)
                {
                    case LegacyWarrantyProposalStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyWarrantyProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy warranty proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy warranty proposal"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Capacity + Ratio, 2);
    }

    public LegacyWarrantyProposal Copy()
    {
        return new LegacyWarrantyProposal(Code, Title, Description, Team, Capacity, Currency, Ratio, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
