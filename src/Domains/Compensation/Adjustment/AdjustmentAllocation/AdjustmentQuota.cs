namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment quota is in its life cycle.</summary>
public enum AdjustmentQuotaStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adjustment quota as the adjustment team keeps it.</summary>
public sealed class AdjustmentQuota(string code, string name, double threshold, int tenureMonths, double floorValue, double discount, string contactEmail, string managerId, AdjustmentQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public int TenureMonths { get; set; } = tenureMonths;
    public double FloorValue { get; set; } = floorValue;
    public double Discount { get; set; } = discount;
    public string ContactEmail { get; set; } = contactEmail;
    public string ManagerId { get; set; } = managerId;
    public AdjustmentQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment quota in status requested.</summary>
    public static AdjustmentQuota NewAdjustmentQuota(string code, string name)
    {
        return new AdjustmentQuota(code, name, 0.0, 0, 0.0, 0.0, "", "", AdjustmentQuotaStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentQuotaStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentQuotaStatus next)
    {
        switch (Status)
        {
            case AdjustmentQuotaStatus.Requested:
                switch (next)
                {
                    case AdjustmentQuotaStatus.Revoked:
                        return true;
                }
                return false;
            case AdjustmentQuotaStatus.Granted:
                return next == AdjustmentQuotaStatus.Revoked || next == AdjustmentQuotaStatus.Expired;
            case AdjustmentQuotaStatus.Revoked:
                return next == AdjustmentQuotaStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdjustmentQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Threshold + FloorValue, 2);
    }

    public AdjustmentQuota Copy()
    {
        return new AdjustmentQuota(
            Code,
            Name,
            Threshold,
            TenureMonths,
            FloorValue,
            Discount,
            ContactEmail,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
