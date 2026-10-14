namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk proposal is in its life cycle.</summary>
public enum DeskProposalStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a desk proposal and its current status.</summary>
public sealed class DeskProposal(string code, string title, string ownerId, int windowDays, bool active, string salaryBand, string grade, string description, bool visible, DeskProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public int WindowDays { get; set; } = windowDays;
    public bool Active { get; set; } = active;
    public string SalaryBand { get; set; } = salaryBand;
    public string Grade { get; set; } = grade;
    public string Description { get; set; } = description;
    public bool Visible { get; set; } = visible;
    public DeskProposalStatus Status { get; set; } = status;

    /// <summary>Starts a desk proposal in status open.</summary>
    public static DeskProposal NewDeskProposal(string code, string title)
    {
        return new DeskProposal(code, title, "", 0, false, "", "", "", false, DeskProposalStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskProposalStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskProposalStatus next)
    {
        switch (Status)
        {
            case DeskProposalStatus.Open:
                switch (next)
                {
                    case DeskProposalStatus.InReview:
                    case DeskProposalStatus.Archived:
                        return true;
                }
                return false;
            case DeskProposalStatus.InReview:
                switch (next)
                {
                    case DeskProposalStatus.Resolved:
                    case DeskProposalStatus.Reopened:
                        return true;
                }
                return false;
            case DeskProposalStatus.Resolved:
                switch (next)
                {
                    case DeskProposalStatus.Archived:
                        return true;
                }
                return false;
            case DeskProposalStatus.Reopened:
                return next == DeskProposalStatus.Archived;
        }
        return false;
    }

    public void MoveTo(DeskProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public DeskProposal Copy()
    {
        return new DeskProposal(
            Code,
            Title,
            OwnerId,
            WindowDays,
            Active,
            SalaryBand,
            Grade,
            Description,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
