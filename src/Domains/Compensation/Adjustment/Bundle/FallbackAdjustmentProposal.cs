namespace Acme.Workforce.Compensation.Adjustment.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback adjustment proposal is in its life cycle.</summary>
public enum FallbackAdjustmentProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One fallback adjustment proposal, with what the planners need to act on it.</summary>
public sealed class FallbackAdjustmentProposal(string code, string label, int sequence, double fte, int batchSize, int windowDays, bool pinned, double discount, string externalRef, string segment, FallbackAdjustmentProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public double Fte { get; set; } = fte;
    public int BatchSize { get; set; } = batchSize;
    public int WindowDays { get; set; } = windowDays;
    public bool Pinned { get; set; } = pinned;
    public double Discount { get; set; } = discount;
    public string ExternalRef { get; set; } = externalRef;
    public string Segment { get; set; } = segment;
    public FallbackAdjustmentProposalStatus Status { get; set; } = status;

    /// <summary>Starts a fallback adjustment proposal in status planned.</summary>
    public static FallbackAdjustmentProposal NewFallbackAdjustmentProposal(string code, string label)
    {
        return new FallbackAdjustmentProposal(
            code,
            label,
            0,
            0.0,
            0,
            0,
            false,
            0.0,
            "",
            "",
            FallbackAdjustmentProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAdjustmentProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAdjustmentProposalStatus next)
    {
        switch (Status)
        {
            case FallbackAdjustmentProposalStatus.Planned:
                switch (next)
                {
                    case FallbackAdjustmentProposalStatus.Completed:
                        return true;
                }
                return false;
            case FallbackAdjustmentProposalStatus.Scheduled:
                return next == FallbackAdjustmentProposalStatus.Cancelled;
            case FallbackAdjustmentProposalStatus.InProgress:
                switch (next)
                {
                    case FallbackAdjustmentProposalStatus.Completed:
                        return true;
                }
                return false;
            case FallbackAdjustmentProposalStatus.Completed:
                return next == FallbackAdjustmentProposalStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FallbackAdjustmentProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback adjustment proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Discount - (double)WindowDays, 2);
    }

    public FallbackAdjustmentProposal Copy()
    {
        return new FallbackAdjustmentProposal(
            Code,
            Label,
            Sequence,
            Fte,
            BatchSize,
            WindowDays,
            Pinned,
            Discount,
            ExternalRef,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
