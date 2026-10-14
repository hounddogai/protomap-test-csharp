namespace Acme.Workforce.Talent.Pool.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool allocation is in its life cycle.</summary>
public enum PoolAllocationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a pool allocation and its current status.</summary>
public sealed class PoolAllocation(string code, string label, int limit, int createdAt, string channel, string team, double floorValue, string unit, double ceiling, string reviewer, string currency, PoolAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public int CreatedAt { get; set; } = createdAt;
    public string Channel { get; set; } = channel;
    public string Team { get; set; } = team;
    public double FloorValue { get; set; } = floorValue;
    public string Unit { get; set; } = unit;
    public double Ceiling { get; set; } = ceiling;
    public string Reviewer { get; set; } = reviewer;
    public string Currency { get; set; } = currency;
    public PoolAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a pool allocation in status new.</summary>
    public static PoolAllocation NewPoolAllocation(string code, string label)
    {
        return new PoolAllocation(code, label, 0, 0, "", "", 0.0, "", 0.0, "", "", PoolAllocationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolAllocationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolAllocationStatus next)
    {
        switch (Status)
        {
            case PoolAllocationStatus.New:
                switch (next)
                {
                    case PoolAllocationStatus.Running:
                    case PoolAllocationStatus.Failed:
                        return true;
                }
                return false;
            case PoolAllocationStatus.Queued:
                switch (next)
                {
                    case PoolAllocationStatus.Failed:
                        return true;
                }
                return false;
            case PoolAllocationStatus.Running:
                switch (next)
                {
                    case PoolAllocationStatus.Done:
                        return true;
                }
                return false;
            case PoolAllocationStatus.Done:
                switch (next)
                {
                    case PoolAllocationStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"pool allocation"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(FloorValue - (double)CreatedAt, 2);
    }

    public PoolAllocation Copy()
    {
        return new PoolAllocation(
            Code,
            Label,
            Limit,
            CreatedAt,
            Channel,
            Team,
            FloorValue,
            Unit,
            Ceiling,
            Reviewer,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
