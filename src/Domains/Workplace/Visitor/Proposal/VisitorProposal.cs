namespace Acme.Workforce.Workplace.Visitor.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor proposal is in its life cycle.</summary>
public enum VisitorProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A visitor proposal as the visitor team keeps it.</summary>
public sealed class VisitorProposal(string code, string label, int revision, int leaveDays, double threshold, string ownerId, double discount, bool approved, string description, VisitorProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public int LeaveDays { get; set; } = leaveDays;
    public double Threshold { get; set; } = threshold;
    public string OwnerId { get; set; } = ownerId;
    public double Discount { get; set; } = discount;
    public bool Approved { get; set; } = approved;
    public string Description { get; set; } = description;
    public VisitorProposalStatus Status { get; set; } = status;

    /// <summary>Starts a visitor proposal in status planned.</summary>
    public static VisitorProposal NewVisitorProposal(string code, string label)
    {
        return new VisitorProposal(code, label, 0, 0, 0.0, "", 0.0, false, "", VisitorProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorProposalStatus next)
    {
        switch (Status)
        {
            case VisitorProposalStatus.Planned:
                switch (next)
                {
                    case VisitorProposalStatus.Scheduled:
                        return true;
                }
                return false;
            case VisitorProposalStatus.Scheduled:
                switch (next)
                {
                    case VisitorProposalStatus.InProgress:
                    case VisitorProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case VisitorProposalStatus.InProgress:
                switch (next)
                {
                    case VisitorProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case VisitorProposalStatus.Completed:
                switch (next)
                {
                    case VisitorProposalStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Threshold + Discount, 2);
    }

    public VisitorProposal Copy()
    {
        return new VisitorProposal(
            Code,
            Label,
            Revision,
            LeaveDays,
            Threshold,
            OwnerId,
            Discount,
            Approved,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
