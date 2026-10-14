namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary position proposal is in its life cycle.</summary>
public enum PrimaryPositionProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A primary position proposal as the position team keeps it.</summary>
public sealed class PrimaryPositionProposal(string code, string label, int startsAt, int endsAt, int priority, PrimaryPositionProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public int EndsAt { get; set; } = endsAt;
    public int Priority { get; set; } = priority;
    public PrimaryPositionProposalStatus Status { get; set; } = status;

    /// <summary>Starts a primary position proposal in status draft.</summary>
    public static PrimaryPositionProposal NewPrimaryPositionProposal(string code, string label)
    {
        return new PrimaryPositionProposal(code, label, 0, 0, 0, PrimaryPositionProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPositionProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPositionProposalStatus next)
    {
        switch (Status)
        {
            case PrimaryPositionProposalStatus.Draft:
                return next == PrimaryPositionProposalStatus.Suspended;
            case PrimaryPositionProposalStatus.Pending:
                switch (next)
                {
                    case PrimaryPositionProposalStatus.Suspended:
                    case PrimaryPositionProposalStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryPositionProposalStatus.Active:
                switch (next)
                {
                    case PrimaryPositionProposalStatus.Suspended:
                        return true;
                }
                return false;
            case PrimaryPositionProposalStatus.Suspended:
                switch (next)
                {
                    case PrimaryPositionProposalStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryPositionProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary position proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"primary position proposal"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Priority - (double)EndsAt, 2);
    }

    public PrimaryPositionProposal Copy()
    {
        return new PrimaryPositionProposal(Code, Label, StartsAt, EndsAt, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
