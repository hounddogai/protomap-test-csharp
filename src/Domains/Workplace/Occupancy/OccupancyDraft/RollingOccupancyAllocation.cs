namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling occupancy allocation is in its life cycle.</summary>
public enum RollingOccupancyAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One rolling occupancy allocation, with what the planners need to act on it.</summary>
public sealed class RollingOccupancyAllocation(string code, string name, string badge, int batchSize, double ratio, double amount, string department, string phone, RollingOccupancyAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public int BatchSize { get; set; } = batchSize;
    public double Ratio { get; set; } = ratio;
    public double Amount { get; set; } = amount;
    public string Department { get; set; } = department;
    public string Phone { get; set; } = phone;
    public RollingOccupancyAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a rolling occupancy allocation in status idle.</summary>
    public static RollingOccupancyAllocation NewRollingOccupancyAllocation(string code, string name)
    {
        return new RollingOccupancyAllocation(
            code,
            name,
            "",
            0,
            0.0,
            0.0,
            "",
            "",
            RollingOccupancyAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingOccupancyAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingOccupancyAllocationStatus next)
    {
        switch (Status)
        {
            case RollingOccupancyAllocationStatus.Idle:
                switch (next)
                {
                    case RollingOccupancyAllocationStatus.Ready:
                    case RollingOccupancyAllocationStatus.Draining:
                        return true;
                }
                return false;
            case RollingOccupancyAllocationStatus.Warming:
                return next == RollingOccupancyAllocationStatus.Ready || next == RollingOccupancyAllocationStatus.Draining;
            case RollingOccupancyAllocationStatus.Ready:
                return next == RollingOccupancyAllocationStatus.Draining || next == RollingOccupancyAllocationStatus.Stopped;
            case RollingOccupancyAllocationStatus.Draining:
                switch (next)
                {
                    case RollingOccupancyAllocationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingOccupancyAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling occupancy allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling occupancy allocation"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Amount + (double)BatchSize, 2);
    }

    public RollingOccupancyAllocation Copy()
    {
        return new RollingOccupancyAllocation(
            Code,
            Name,
            Badge,
            BatchSize,
            Ratio,
            Amount,
            Department,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
