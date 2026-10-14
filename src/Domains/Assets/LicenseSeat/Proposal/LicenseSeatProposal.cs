namespace Acme.Workforce.Assets.LicenseSeat.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat proposal is in its life cycle.</summary>
public enum LicenseSeatProposalStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a license seat proposal and its current status.</summary>
public sealed class LicenseSeatProposal(string code, string label, int batchSize, bool archived, string location, LicenseSeatProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public bool Archived { get; set; } = archived;
    public string Location { get; set; } = location;
    public LicenseSeatProposalStatus Status { get; set; } = status;

    /// <summary>Starts a license seat proposal in status requested.</summary>
    public static LicenseSeatProposal NewLicenseSeatProposal(string code, string label)
    {
        return new LicenseSeatProposal(code, label, 0, false, "", LicenseSeatProposalStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatProposalStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatProposalStatus next)
    {
        switch (Status)
        {
            case LicenseSeatProposalStatus.Requested:
                switch (next)
                {
                    case LicenseSeatProposalStatus.Expired:
                        return true;
                }
                return false;
            case LicenseSeatProposalStatus.Granted:
                return next == LicenseSeatProposalStatus.Revoked || next == LicenseSeatProposalStatus.Expired;
            case LicenseSeatProposalStatus.Revoked:
                return next == LicenseSeatProposalStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LicenseSeatProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public LicenseSeatProposal Copy()
    {
        return new LicenseSeatProposal(Code, Label, BatchSize, Archived, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
