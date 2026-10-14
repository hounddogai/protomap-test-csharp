namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending exception proposal is in its life cycle.</summary>
public enum PendingExceptionProposalStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a pending exception proposal and its current status.</summary>
public sealed class PendingExceptionProposal(string code, string title, int attempts, string reviewer, double rate, PendingExceptionProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public string Reviewer { get; set; } = reviewer;
    public double Rate { get; set; } = rate;
    public PendingExceptionProposalStatus Status { get; set; } = status;

    /// <summary>Starts a pending exception proposal in status requested.</summary>
    public static PendingExceptionProposal NewPendingExceptionProposal(string code, string title)
    {
        return new PendingExceptionProposal(code, title, 0, "", 0.0, PendingExceptionProposalStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingExceptionProposalStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingExceptionProposalStatus next)
    {
        switch (Status)
        {
            case PendingExceptionProposalStatus.Requested:
                return next == PendingExceptionProposalStatus.Revoked || next == PendingExceptionProposalStatus.Expired;
            case PendingExceptionProposalStatus.Granted:
                switch (next)
                {
                    case PendingExceptionProposalStatus.Revoked:
                    case PendingExceptionProposalStatus.Expired:
                        return true;
                }
                return false;
            case PendingExceptionProposalStatus.Revoked:
                return next == PendingExceptionProposalStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingExceptionProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending exception proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Rate * (double)Attempts, 2);
    }

    public PendingExceptionProposal Copy()
    {
        return new PendingExceptionProposal(Code, Title, Attempts, Reviewer, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
