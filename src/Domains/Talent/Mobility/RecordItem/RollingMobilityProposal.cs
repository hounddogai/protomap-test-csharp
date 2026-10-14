namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling mobility proposal is in its life cycle.</summary>
public enum RollingMobilityProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A rolling mobility proposal. Its code is unique within a tenant.</summary>
public sealed class RollingMobilityProposal(string code, string label, int expiresAt, string notes, string source, RollingMobilityProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Notes { get; set; } = notes;
    public string Source { get; set; } = source;
    public RollingMobilityProposalStatus Status { get; set; } = status;

    /// <summary>Starts a rolling mobility proposal in status planned.</summary>
    public static RollingMobilityProposal NewRollingMobilityProposal(string code, string label)
    {
        return new RollingMobilityProposal(code, label, 0, "", "", RollingMobilityProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingMobilityProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingMobilityProposalStatus next)
    {
        switch (Status)
        {
            case RollingMobilityProposalStatus.Planned:
                switch (next)
                {
                    case RollingMobilityProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingMobilityProposalStatus.Scheduled:
                switch (next)
                {
                    case RollingMobilityProposalStatus.Completed:
                    case RollingMobilityProposalStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingMobilityProposalStatus.InProgress:
                return next == RollingMobilityProposalStatus.Completed || next == RollingMobilityProposalStatus.Cancelled;
            case RollingMobilityProposalStatus.Completed:
                switch (next)
                {
                    case RollingMobilityProposalStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingMobilityProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling mobility proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public RollingMobilityProposal Copy()
    {
        return new RollingMobilityProposal(Code, Label, ExpiresAt, Notes, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
