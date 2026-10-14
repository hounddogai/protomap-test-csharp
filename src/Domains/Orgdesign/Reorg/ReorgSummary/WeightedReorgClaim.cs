namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted reorg claim is in its life cycle.</summary>
public enum WeightedReorgClaimStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a weighted reorg claim and its current status.</summary>
public sealed class WeightedReorgClaim(string code, string name, double threshold, double fte, int sequence, bool active, WeightedReorgClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public double Fte { get; set; } = fte;
    public int Sequence { get; set; } = sequence;
    public bool Active { get; set; } = active;
    public WeightedReorgClaimStatus Status { get; set; } = status;

    /// <summary>Starts a weighted reorg claim in status requested.</summary>
    public static WeightedReorgClaim NewWeightedReorgClaim(string code, string name)
    {
        return new WeightedReorgClaim(code, name, 0.0, 0.0, 0, false, WeightedReorgClaimStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedReorgClaimStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedReorgClaimStatus next)
    {
        switch (Status)
        {
            case WeightedReorgClaimStatus.Requested:
                return next == WeightedReorgClaimStatus.Expired;
            case WeightedReorgClaimStatus.Granted:
                switch (next)
                {
                    case WeightedReorgClaimStatus.Revoked:
                    case WeightedReorgClaimStatus.Expired:
                        return true;
                }
                return false;
            case WeightedReorgClaimStatus.Revoked:
                switch (next)
                {
                    case WeightedReorgClaimStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedReorgClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted reorg claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Threshold + Fte, 2);
    }

    public WeightedReorgClaim Copy()
    {
        return new WeightedReorgClaim(Code, Name, Threshold, Fte, Sequence, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
