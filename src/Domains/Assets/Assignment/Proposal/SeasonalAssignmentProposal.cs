namespace Acme.Workforce.Assets.Assignment.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal assignment proposal is in its life cycle.</summary>
public enum SeasonalAssignmentProposalStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A seasonal assignment proposal. Its code is unique within a tenant.</summary>
public sealed class SeasonalAssignmentProposal(string code, string name, int durationMinutes, int endsAt, bool approved, double hours, double minimum, string costCenter, string country, SeasonalAssignmentProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int EndsAt { get; set; } = endsAt;
    public bool Approved { get; set; } = approved;
    public double Hours { get; set; } = hours;
    public double Minimum { get; set; } = minimum;
    public string CostCenter { get; set; } = costCenter;
    public string Country { get; set; } = country;
    public SeasonalAssignmentProposalStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal assignment proposal in status planned.</summary>
    public static SeasonalAssignmentProposal NewSeasonalAssignmentProposal(string code, string name)
    {
        return new SeasonalAssignmentProposal(
            code,
            name,
            0,
            0,
            false,
            0.0,
            0.0,
            "",
            "",
            SeasonalAssignmentProposalStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAssignmentProposalStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAssignmentProposalStatus next)
    {
        switch (Status)
        {
            case SeasonalAssignmentProposalStatus.Planned:
                return next == SeasonalAssignmentProposalStatus.InProgress;
            case SeasonalAssignmentProposalStatus.Scheduled:
                return next == SeasonalAssignmentProposalStatus.InProgress || next == SeasonalAssignmentProposalStatus.Cancelled;
            case SeasonalAssignmentProposalStatus.InProgress:
                return next == SeasonalAssignmentProposalStatus.Cancelled;
            case SeasonalAssignmentProposalStatus.Completed:
                switch (next)
                {
                    case SeasonalAssignmentProposalStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalAssignmentProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal assignment proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"seasonal assignment proposal"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Minimum - Hours, 2);
    }

    public SeasonalAssignmentProposal Copy()
    {
        return new SeasonalAssignmentProposal(
            Code,
            Name,
            DurationMinutes,
            EndsAt,
            Approved,
            Hours,
            Minimum,
            CostCenter,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
