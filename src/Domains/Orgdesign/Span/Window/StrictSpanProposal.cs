namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict span proposal is in its life cycle.</summary>
public enum StrictSpanProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One strict span proposal, with what the planners need to act on it.</summary>
public sealed class StrictSpanProposal(string code, string title, string skill, bool approved, string notes, double score, double floorValue, StrictSpanProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public bool Approved { get; set; } = approved;
    public string Notes { get; set; } = notes;
    public double Score { get; set; } = score;
    public double FloorValue { get; set; } = floorValue;
    public StrictSpanProposalStatus Status { get; set; } = status;

    /// <summary>Starts a strict span proposal in status planned.</summary>
    public static StrictSpanProposal NewStrictSpanProposal(string code, string title)
    {
        return new StrictSpanProposal(code, title, "", false, "", 0.0, 0.0, StrictSpanProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictSpanProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictSpanProposalStatus next)
    {
        switch (Status)
        {
            case StrictSpanProposalStatus.Planned:
                return next == StrictSpanProposalStatus.Scheduled;
            case StrictSpanProposalStatus.Scheduled:
                switch (next)
                {
                    case StrictSpanProposalStatus.InProgress:
                    case StrictSpanProposalStatus.Completed:
                        return true;
                }
                return false;
            case StrictSpanProposalStatus.InProgress:
                switch (next)
                {
                    case StrictSpanProposalStatus.Completed:
                    case StrictSpanProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case StrictSpanProposalStatus.Completed:
                switch (next)
                {
                    case StrictSpanProposalStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictSpanProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict span proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetValue()
    {
        return Math.Round(FloorValue - Score, 2);
    }

    public StrictSpanProposal Copy()
    {
        return new StrictSpanProposal(Code, Title, Skill, Approved, Notes, Score, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
