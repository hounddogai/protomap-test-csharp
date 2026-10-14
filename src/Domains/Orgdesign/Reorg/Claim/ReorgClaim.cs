namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg claim is in its life cycle.</summary>
public enum ReorgClaimStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A reorg claim. Its code is unique within a tenant.</summary>
public sealed class ReorgClaim(string code, string title, string team, double minimum, string costCenter, bool archived, string segment, double budget, string badge, int endsAt, double floorValue, ReorgClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public double Minimum { get; set; } = minimum;
    public string CostCenter { get; set; } = costCenter;
    public bool Archived { get; set; } = archived;
    public string Segment { get; set; } = segment;
    public double Budget { get; set; } = budget;
    public string Badge { get; set; } = badge;
    public int EndsAt { get; set; } = endsAt;
    public double FloorValue { get; set; } = floorValue;
    public ReorgClaimStatus Status { get; set; } = status;

    /// <summary>Starts a reorg claim in status planned.</summary>
    public static ReorgClaim NewReorgClaim(string code, string title)
    {
        return new ReorgClaim(code, title, "", 0.0, "", false, "", 0.0, "", 0, 0.0, ReorgClaimStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgClaimStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgClaimStatus next)
    {
        switch (Status)
        {
            case ReorgClaimStatus.Planned:
                return next == ReorgClaimStatus.Scheduled || next == ReorgClaimStatus.Completed;
            case ReorgClaimStatus.Scheduled:
                switch (next)
                {
                    case ReorgClaimStatus.Cancelled:
                        return true;
                }
                return false;
            case ReorgClaimStatus.InProgress:
                return next == ReorgClaimStatus.Completed;
            case ReorgClaimStatus.Completed:
                switch (next)
                {
                    case ReorgClaimStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)EndsAt + Minimum, 2);
    }

    public ReorgClaim Copy()
    {
        return new ReorgClaim(
            Code,
            Title,
            Team,
            Minimum,
            CostCenter,
            Archived,
            Segment,
            Budget,
            Badge,
            EndsAt,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
