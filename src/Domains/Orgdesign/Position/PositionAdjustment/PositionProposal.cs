namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position proposal is in its life cycle.</summary>
public enum PositionProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A position proposal as the position team keeps it.</summary>
public sealed class PositionProposal(string code, string label, double margin, string grade, string segment, bool archived, bool approved, int quantity, string jobCode, int graceDays, PositionProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public string Grade { get; set; } = grade;
    public string Segment { get; set; } = segment;
    public bool Archived { get; set; } = archived;
    public bool Approved { get; set; } = approved;
    public int Quantity { get; set; } = quantity;
    public string JobCode { get; set; } = jobCode;
    public int GraceDays { get; set; } = graceDays;
    public PositionProposalStatus Status { get; set; } = status;

    /// <summary>Starts a position proposal in status planned.</summary>
    public static PositionProposal NewPositionProposal(string code, string label)
    {
        return new PositionProposal(
            code,
            label,
            0.0,
            "",
            "",
            false,
            false,
            0,
            "",
            0,
            PositionProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionProposalStatus next)
    {
        switch (Status)
        {
            case PositionProposalStatus.Planned:
                return next == PositionProposalStatus.Scheduled;
            case PositionProposalStatus.Scheduled:
                switch (next)
                {
                    case PositionProposalStatus.InProgress:
                    case PositionProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case PositionProposalStatus.InProgress:
                switch (next)
                {
                    case PositionProposalStatus.Completed:
                    case PositionProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case PositionProposalStatus.Completed:
                switch (next)
                {
                    case PositionProposalStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)GraceDays * Margin, 2);
    }

    public PositionProposal Copy()
    {
        return new PositionProposal(
            Code,
            Label,
            Margin,
            Grade,
            Segment,
            Archived,
            Approved,
            Quantity,
            JobCode,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
