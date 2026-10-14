namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary reorg claim is in its life cycle.</summary>
public enum PrimaryReorgClaimStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A primary reorg claim. Its code is unique within a tenant.</summary>
public sealed class PrimaryReorgClaim(string code, string label, int graceDays, double ceiling, bool pinned, string source, double floorValue, string location, double score, int endsAt, int startsAt, PrimaryReorgClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public double Ceiling { get; set; } = ceiling;
    public bool Pinned { get; set; } = pinned;
    public string Source { get; set; } = source;
    public double FloorValue { get; set; } = floorValue;
    public string Location { get; set; } = location;
    public double Score { get; set; } = score;
    public int EndsAt { get; set; } = endsAt;
    public int StartsAt { get; set; } = startsAt;
    public PrimaryReorgClaimStatus Status { get; set; } = status;

    /// <summary>Starts a primary reorg claim in status planned.</summary>
    public static PrimaryReorgClaim NewPrimaryReorgClaim(string code, string label)
    {
        return new PrimaryReorgClaim(
            code,
            label,
            0,
            0.0,
            false,
            "",
            0.0,
            "",
            0.0,
            0,
            0,
            PrimaryReorgClaimStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryReorgClaimStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryReorgClaimStatus next)
    {
        switch (Status)
        {
            case PrimaryReorgClaimStatus.Planned:
                switch (next)
                {
                    case PrimaryReorgClaimStatus.InProgress:
                    case PrimaryReorgClaimStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryReorgClaimStatus.Scheduled:
                switch (next)
                {
                    case PrimaryReorgClaimStatus.InProgress:
                        return true;
                }
                return false;
            case PrimaryReorgClaimStatus.InProgress:
                switch (next)
                {
                    case PrimaryReorgClaimStatus.Completed:
                        return true;
                }
                return false;
            case PrimaryReorgClaimStatus.Completed:
                switch (next)
                {
                    case PrimaryReorgClaimStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryReorgClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary reorg claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"primary reorg claim"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Score - Ceiling, 2);
    }

    public PrimaryReorgClaim Copy()
    {
        return new PrimaryReorgClaim(
            Code,
            Label,
            GraceDays,
            Ceiling,
            Pinned,
            Source,
            FloorValue,
            Location,
            Score,
            EndsAt,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
