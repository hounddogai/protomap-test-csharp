namespace Acme.Workforce.Payrollops.PayRun.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending pay run allocation is in its life cycle.</summary>
public enum PendingPayRunAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A pending pay run allocation. Its code is unique within a tenant.</summary>
public sealed class PendingPayRunAllocation(string code, string name, double margin, double weight, int endsAt, string country, int durationMinutes, double floorValue, int batchSize, double ratio, PendingPayRunAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public double Weight { get; set; } = weight;
    public int EndsAt { get; set; } = endsAt;
    public string Country { get; set; } = country;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double FloorValue { get; set; } = floorValue;
    public int BatchSize { get; set; } = batchSize;
    public double Ratio { get; set; } = ratio;
    public PendingPayRunAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a pending pay run allocation in status idle.</summary>
    public static PendingPayRunAllocation NewPendingPayRunAllocation(string code, string name)
    {
        return new PendingPayRunAllocation(
            code,
            name,
            0.0,
            0.0,
            0,
            "",
            0,
            0.0,
            0,
            0.0,
            PendingPayRunAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPayRunAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPayRunAllocationStatus next)
    {
        switch (Status)
        {
            case PendingPayRunAllocationStatus.Idle:
                return next == PendingPayRunAllocationStatus.Warming || next == PendingPayRunAllocationStatus.Draining;
            case PendingPayRunAllocationStatus.Warming:
                return next == PendingPayRunAllocationStatus.Ready || next == PendingPayRunAllocationStatus.Stopped;
            case PendingPayRunAllocationStatus.Ready:
                return next == PendingPayRunAllocationStatus.Draining || next == PendingPayRunAllocationStatus.Stopped;
            case PendingPayRunAllocationStatus.Draining:
                switch (next)
                {
                    case PendingPayRunAllocationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingPayRunAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending pay run allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pending pay run allocation"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)BatchSize * Margin, 2);
    }

    public PendingPayRunAllocation Copy()
    {
        return new PendingPayRunAllocation(
            Code,
            Name,
            Margin,
            Weight,
            EndsAt,
            Country,
            DurationMinutes,
            FloorValue,
            BatchSize,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
