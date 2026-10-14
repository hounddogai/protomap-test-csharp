namespace Acme.Workforce.Compensation.Adjustment.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal adjustment proposal is in its life cycle.</summary>
public enum SeasonalAdjustmentProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A seasonal adjustment proposal. Its code is unique within a tenant.</summary>
public sealed class SeasonalAdjustmentProposal(string code, string label, int quantity, double hours, string jobCode, string title, string ownerId, double discount, string timezone, string badge, List<string> tags, SeasonalAdjustmentProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public double Hours { get; set; } = hours;
    public string JobCode { get; set; } = jobCode;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public double Discount { get; set; } = discount;
    public string Timezone { get; set; } = timezone;
    public string Badge { get; set; } = badge;
    public List<string> Tags { get; set; } = tags;
    public SeasonalAdjustmentProposalStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal adjustment proposal in status planned.</summary>
    public static SeasonalAdjustmentProposal NewSeasonalAdjustmentProposal(string code, string label)
    {
        return new SeasonalAdjustmentProposal(
            code,
            label,
            0,
            0.0,
            "",
            "",
            "",
            0.0,
            "",
            "",
            new List<string>(),
            SeasonalAdjustmentProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAdjustmentProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAdjustmentProposalStatus next)
    {
        switch (Status)
        {
            case SeasonalAdjustmentProposalStatus.Planned:
                return next == SeasonalAdjustmentProposalStatus.Scheduled || next == SeasonalAdjustmentProposalStatus.InProgress;
            case SeasonalAdjustmentProposalStatus.Scheduled:
                return next == SeasonalAdjustmentProposalStatus.Cancelled;
            case SeasonalAdjustmentProposalStatus.InProgress:
                return next == SeasonalAdjustmentProposalStatus.Cancelled;
            case SeasonalAdjustmentProposalStatus.Completed:
                switch (next)
                {
                    case SeasonalAdjustmentProposalStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalAdjustmentProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal adjustment proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Quantity - Discount, 2);
    }

    public SeasonalAdjustmentProposal Copy()
    {
        return new SeasonalAdjustmentProposal(
            Code,
            Label,
            Quantity,
            Hours,
            JobCode,
            Title,
            OwnerId,
            Discount,
            Timezone,
            Badge,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
