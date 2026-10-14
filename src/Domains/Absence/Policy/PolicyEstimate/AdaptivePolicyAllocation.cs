namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive policy allocation is in its life cycle.</summary>
public enum AdaptivePolicyAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adaptive policy allocation as the policy team keeps it.</summary>
public sealed class AdaptivePolicyAllocation(string code, string label, int capacity, int quantity, int batchSize, double weight, int priority, double rate, int updatedAt, double ceiling, string unit, AdaptivePolicyAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public int Quantity { get; set; } = quantity;
    public int BatchSize { get; set; } = batchSize;
    public double Weight { get; set; } = weight;
    public int Priority { get; set; } = priority;
    public double Rate { get; set; } = rate;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Ceiling { get; set; } = ceiling;
    public string Unit { get; set; } = unit;
    public AdaptivePolicyAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive policy allocation in status requested.</summary>
    public static AdaptivePolicyAllocation NewAdaptivePolicyAllocation(string code, string label)
    {
        return new AdaptivePolicyAllocation(
            code,
            label,
            0,
            0,
            0,
            0.0,
            0,
            0.0,
            0,
            0.0,
            "",
            AdaptivePolicyAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePolicyAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePolicyAllocationStatus next)
    {
        switch (Status)
        {
            case AdaptivePolicyAllocationStatus.Requested:
                switch (next)
                {
                    case AdaptivePolicyAllocationStatus.Expired:
                        return true;
                }
                return false;
            case AdaptivePolicyAllocationStatus.Granted:
                return next == AdaptivePolicyAllocationStatus.Revoked;
            case AdaptivePolicyAllocationStatus.Revoked:
                switch (next)
                {
                    case AdaptivePolicyAllocationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptivePolicyAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive policy allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Quantity + (double)Priority, 2);
    }

    public AdaptivePolicyAllocation Copy()
    {
        return new AdaptivePolicyAllocation(
            Code,
            Label,
            Capacity,
            Quantity,
            BatchSize,
            Weight,
            Priority,
            Rate,
            UpdatedAt,
            Ceiling,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
