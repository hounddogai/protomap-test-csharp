namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment allocation is in its life cycle.</summary>
public enum AdjustmentAllocationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A adjustment allocation. Its code is unique within a tenant.</summary>
public sealed class AdjustmentAllocation(string code, string name, double ratio, string timezone, string currency, int sequence, int updatedAt, int expiresAt, string location, int tenureMonths, int endsAt, AdjustmentAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public string Timezone { get; set; } = timezone;
    public string Currency { get; set; } = currency;
    public int Sequence { get; set; } = sequence;
    public int UpdatedAt { get; set; } = updatedAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Location { get; set; } = location;
    public int TenureMonths { get; set; } = tenureMonths;
    public int EndsAt { get; set; } = endsAt;
    public AdjustmentAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment allocation in status proposed.</summary>
    public static AdjustmentAllocation NewAdjustmentAllocation(string code, string name)
    {
        return new AdjustmentAllocation(
            code,
            name,
            0.0,
            "",
            "",
            0,
            0,
            0,
            "",
            0,
            0,
            AdjustmentAllocationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentAllocationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentAllocationStatus next)
    {
        switch (Status)
        {
            case AdjustmentAllocationStatus.Proposed:
                return next == AdjustmentAllocationStatus.Approved || next == AdjustmentAllocationStatus.Rejected;
            case AdjustmentAllocationStatus.Approved:
                return next == AdjustmentAllocationStatus.Rejected || next == AdjustmentAllocationStatus.Withdrawn;
            case AdjustmentAllocationStatus.Rejected:
                switch (next)
                {
                    case AdjustmentAllocationStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"adjustment allocation"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)UpdatedAt - (double)TenureMonths, 2);
    }

    public AdjustmentAllocation Copy()
    {
        return new AdjustmentAllocation(
            Code,
            Name,
            Ratio,
            Timezone,
            Currency,
            Sequence,
            UpdatedAt,
            ExpiresAt,
            Location,
            TenureMonths,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
