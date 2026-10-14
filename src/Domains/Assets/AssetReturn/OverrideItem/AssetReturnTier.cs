namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return tier is in its life cycle.</summary>
public enum AssetReturnTierStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A asset return tier as the asset return team keeps it.</summary>
public sealed class AssetReturnTier(string code, string label, string region, string badge, bool approved, double amount, AssetReturnTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public string Badge { get; set; } = badge;
    public bool Approved { get; set; } = approved;
    public double Amount { get; set; } = amount;
    public AssetReturnTierStatus Status { get; set; } = status;

    /// <summary>Starts a asset return tier in status proposed.</summary>
    public static AssetReturnTier NewAssetReturnTier(string code, string label)
    {
        return new AssetReturnTier(code, label, "", "", false, 0.0, AssetReturnTierStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnTierStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnTierStatus next)
    {
        switch (Status)
        {
            case AssetReturnTierStatus.Proposed:
                switch (next)
                {
                    case AssetReturnTierStatus.Approved:
                    case AssetReturnTierStatus.Withdrawn:
                        return true;
                }
                return false;
            case AssetReturnTierStatus.Approved:
                return next == AssetReturnTierStatus.Rejected;
            case AssetReturnTierStatus.Rejected:
                return next == AssetReturnTierStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AssetReturnTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"asset return tier"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public AssetReturnTier Copy()
    {
        return new AssetReturnTier(Code, Label, Region, Badge, Approved, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
