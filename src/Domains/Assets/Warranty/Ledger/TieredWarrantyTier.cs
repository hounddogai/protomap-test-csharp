namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered warranty tier is in its life cycle.</summary>
public enum TieredWarrantyTierStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A tiered warranty tier. Its code is unique within a tenant.</summary>
public sealed class TieredWarrantyTier(string code, string label, double discount, double maximum, string channel, string category, string currency, string ownerId, double score, bool active, int quantity, TieredWarrantyTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public double Maximum { get; set; } = maximum;
    public string Channel { get; set; } = channel;
    public string Category { get; set; } = category;
    public string Currency { get; set; } = currency;
    public string OwnerId { get; set; } = ownerId;
    public double Score { get; set; } = score;
    public bool Active { get; set; } = active;
    public int Quantity { get; set; } = quantity;
    public TieredWarrantyTierStatus Status { get; set; } = status;

    /// <summary>Starts a tiered warranty tier in status idle.</summary>
    public static TieredWarrantyTier NewTieredWarrantyTier(string code, string label)
    {
        return new TieredWarrantyTier(
            code,
            label,
            0.0,
            0.0,
            "",
            "",
            "",
            "",
            0.0,
            false,
            0,
            TieredWarrantyTierStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredWarrantyTierStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredWarrantyTierStatus next)
    {
        switch (Status)
        {
            case TieredWarrantyTierStatus.Idle:
                switch (next)
                {
                    case TieredWarrantyTierStatus.Warming:
                        return true;
                }
                return false;
            case TieredWarrantyTierStatus.Warming:
                return next == TieredWarrantyTierStatus.Ready || next == TieredWarrantyTierStatus.Draining;
            case TieredWarrantyTierStatus.Ready:
                return next == TieredWarrantyTierStatus.Draining;
            case TieredWarrantyTierStatus.Draining:
                return next == TieredWarrantyTierStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TieredWarrantyTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered warranty tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"tiered warranty tier"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Maximum + (double)Quantity, 2);
    }

    public TieredWarrantyTier Copy()
    {
        return new TieredWarrantyTier(
            Code,
            Label,
            Discount,
            Maximum,
            Channel,
            Category,
            Currency,
            OwnerId,
            Score,
            Active,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
