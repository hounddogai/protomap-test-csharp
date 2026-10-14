namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall proposal is in its life cycle.</summary>
public enum TownHallProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One town hall proposal, with what the planners need to act on it.</summary>
public sealed class TownHallProposal(string code, string name, int createdAt, string skill, int sequence, double fte, int version, TownHallProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public string Skill { get; set; } = skill;
    public int Sequence { get; set; } = sequence;
    public double Fte { get; set; } = fte;
    public int Version { get; set; } = version;
    public TownHallProposalStatus Status { get; set; } = status;

    /// <summary>Starts a town hall proposal in status planned.</summary>
    public static TownHallProposal NewTownHallProposal(string code, string name)
    {
        return new TownHallProposal(code, name, 0, "", 0, 0.0, 0, TownHallProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallProposalStatus next)
    {
        switch (Status)
        {
            case TownHallProposalStatus.Planned:
                switch (next)
                {
                    case TownHallProposalStatus.Completed:
                    case TownHallProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case TownHallProposalStatus.Scheduled:
                return next == TownHallProposalStatus.InProgress || next == TownHallProposalStatus.Completed;
            case TownHallProposalStatus.InProgress:
                return next == TownHallProposalStatus.Completed;
            case TownHallProposalStatus.Completed:
                return next == TownHallProposalStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TownHallProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"town hall proposal"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Version + (double)CreatedAt, 2);
    }

    public TownHallProposal Copy()
    {
        return new TownHallProposal(Code, Name, CreatedAt, Skill, Sequence, Fte, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
