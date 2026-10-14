namespace Acme.Workforce.Compensation.Adjustment.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment proposal is in its life cycle.</summary>
public enum AdjustmentProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a adjustment proposal and its current status.</summary>
public sealed class AdjustmentProposal(string code, string name, string label, int startsAt, int sequence, int windowDays, string ownerId, double minimum, string managerId, AdjustmentProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public int Sequence { get; set; } = sequence;
    public int WindowDays { get; set; } = windowDays;
    public string OwnerId { get; set; } = ownerId;
    public double Minimum { get; set; } = minimum;
    public string ManagerId { get; set; } = managerId;
    public AdjustmentProposalStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment proposal in status planned.</summary>
    public static AdjustmentProposal NewAdjustmentProposal(string code, string name)
    {
        return new AdjustmentProposal(code, name, "", 0, 0, 0, "", 0.0, "", AdjustmentProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentProposalStatus next)
    {
        switch (Status)
        {
            case AdjustmentProposalStatus.Planned:
                return next == AdjustmentProposalStatus.Cancelled;
            case AdjustmentProposalStatus.Scheduled:
                switch (next)
                {
                    case AdjustmentProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case AdjustmentProposalStatus.InProgress:
                switch (next)
                {
                    case AdjustmentProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case AdjustmentProposalStatus.Completed:
                return next == AdjustmentProposalStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AdjustmentProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)StartsAt - Minimum, 2);
    }

    public AdjustmentProposal Copy()
    {
        return new AdjustmentProposal(
            Code,
            Name,
            Label,
            StartsAt,
            Sequence,
            WindowDays,
            OwnerId,
            Minimum,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
