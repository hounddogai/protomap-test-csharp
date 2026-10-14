namespace Acme.Workforce.Compensation.MeritCycle.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict merit cycle proposal is in its life cycle.</summary>
public enum StrictMeritCycleProposalStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a strict merit cycle proposal and its current status.</summary>
public sealed class StrictMeritCycleProposal(string code, string name, string currency, double margin, int attempts, StrictMeritCycleProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public double Margin { get; set; } = margin;
    public int Attempts { get; set; } = attempts;
    public StrictMeritCycleProposalStatus Status { get; set; } = status;

    /// <summary>Starts a strict merit cycle proposal in status requested.</summary>
    public static StrictMeritCycleProposal NewStrictMeritCycleProposal(string code, string name)
    {
        return new StrictMeritCycleProposal(code, name, "", 0.0, 0, StrictMeritCycleProposalStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictMeritCycleProposalStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictMeritCycleProposalStatus next)
    {
        switch (Status)
        {
            case StrictMeritCycleProposalStatus.Requested:
                return next == StrictMeritCycleProposalStatus.Granted || next == StrictMeritCycleProposalStatus.Expired;
            case StrictMeritCycleProposalStatus.Granted:
                switch (next)
                {
                    case StrictMeritCycleProposalStatus.Expired:
                        return true;
                }
                return false;
            case StrictMeritCycleProposalStatus.Revoked:
                return next == StrictMeritCycleProposalStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StrictMeritCycleProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict merit cycle proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict merit cycle proposal"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Margin - (double)Attempts, 2);
    }

    public StrictMeritCycleProposal Copy()
    {
        return new StrictMeritCycleProposal(Code, Name, Currency, Margin, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
