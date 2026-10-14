namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy proposal is in its life cycle.</summary>
public enum OccupancyProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A occupancy proposal. Its code is unique within a tenant.</summary>
public sealed class OccupancyProposal(string code, string label, int limit, bool archived, string summary, double fte, string jobCode, string location, OccupancyProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public bool Archived { get; set; } = archived;
    public string Summary { get; set; } = summary;
    public double Fte { get; set; } = fte;
    public string JobCode { get; set; } = jobCode;
    public string Location { get; set; } = location;
    public OccupancyProposalStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy proposal in status draft.</summary>
    public static OccupancyProposal NewOccupancyProposal(string code, string label)
    {
        return new OccupancyProposal(code, label, 0, false, "", 0.0, "", "", OccupancyProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyProposalStatus next)
    {
        switch (Status)
        {
            case OccupancyProposalStatus.Draft:
                return next == OccupancyProposalStatus.Suspended;
            case OccupancyProposalStatus.Pending:
                return next == OccupancyProposalStatus.Suspended;
            case OccupancyProposalStatus.Active:
                return next == OccupancyProposalStatus.Suspended || next == OccupancyProposalStatus.Closed;
            case OccupancyProposalStatus.Suspended:
                return next == OccupancyProposalStatus.Closed;
        }
        return false;
    }

    public void MoveTo(OccupancyProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedLoad()
    {
        return Math.Round(Fte + (double)Limit, 2);
    }

    public OccupancyProposal Copy()
    {
        return new OccupancyProposal(Code, Label, Limit, Archived, Summary, Fte, JobCode, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
