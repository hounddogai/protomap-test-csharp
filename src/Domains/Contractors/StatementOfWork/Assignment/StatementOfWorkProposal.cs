namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work proposal is in its life cycle.</summary>
public enum StatementOfWorkProposalStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One statement of work proposal, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkProposal(string code, string title, int startsAt, int windowDays, string grade, string skill, int revision, string description, double minimum, string ownerId, string currency, StatementOfWorkProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public int WindowDays { get; set; } = windowDays;
    public string Grade { get; set; } = grade;
    public string Skill { get; set; } = skill;
    public int Revision { get; set; } = revision;
    public string Description { get; set; } = description;
    public double Minimum { get; set; } = minimum;
    public string OwnerId { get; set; } = ownerId;
    public string Currency { get; set; } = currency;
    public StatementOfWorkProposalStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work proposal in status open.</summary>
    public static StatementOfWorkProposal NewStatementOfWorkProposal(string code, string title)
    {
        return new StatementOfWorkProposal(
            code,
            title,
            0,
            0,
            "",
            "",
            0,
            "",
            0.0,
            "",
            "",
            StatementOfWorkProposalStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkProposalStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkProposalStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkProposalStatus.Open:
                switch (next)
                {
                    case StatementOfWorkProposalStatus.Resolved:
                        return true;
                }
                return false;
            case StatementOfWorkProposalStatus.InReview:
                return next == StatementOfWorkProposalStatus.Reopened || next == StatementOfWorkProposalStatus.Archived;
            case StatementOfWorkProposalStatus.Resolved:
                switch (next)
                {
                    case StatementOfWorkProposalStatus.Reopened:
                        return true;
                }
                return false;
            case StatementOfWorkProposalStatus.Reopened:
                return next == StatementOfWorkProposalStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Revision + (double)WindowDays, 2);
    }

    public StatementOfWorkProposal Copy()
    {
        return new StatementOfWorkProposal(
            Code,
            Title,
            StartsAt,
            WindowDays,
            Grade,
            Skill,
            Revision,
            Description,
            Minimum,
            OwnerId,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
