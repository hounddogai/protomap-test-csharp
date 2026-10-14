namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return claim is in its life cycle.</summary>
public enum AssetReturnClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A asset return claim as the asset return team keeps it.</summary>
public sealed class AssetReturnClaim(string code, string label, string phone, double cost, int windowDays, AssetReturnClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public double Cost { get; set; } = cost;
    public int WindowDays { get; set; } = windowDays;
    public AssetReturnClaimStatus Status { get; set; } = status;

    /// <summary>Starts a asset return claim in status new.</summary>
    public static AssetReturnClaim NewAssetReturnClaim(string code, string label)
    {
        return new AssetReturnClaim(code, label, "", 0.0, 0, AssetReturnClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnClaimStatus next)
    {
        switch (Status)
        {
            case AssetReturnClaimStatus.New:
                return next == AssetReturnClaimStatus.Queued || next == AssetReturnClaimStatus.Done;
            case AssetReturnClaimStatus.Queued:
                return next == AssetReturnClaimStatus.Done || next == AssetReturnClaimStatus.Failed;
            case AssetReturnClaimStatus.Running:
                return next == AssetReturnClaimStatus.Failed;
            case AssetReturnClaimStatus.Done:
                switch (next)
                {
                    case AssetReturnClaimStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"asset return claim"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)WindowDays + Cost, 2);
    }

    public AssetReturnClaim Copy()
    {
        return new AssetReturnClaim(Code, Label, Phone, Cost, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
