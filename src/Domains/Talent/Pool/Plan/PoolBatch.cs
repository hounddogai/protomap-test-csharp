namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool batch is in its life cycle.</summary>
public enum PoolBatchStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pool batch as the pool team keeps it.</summary>
public sealed class PoolBatch(string code, string name, bool active, string location, double ceiling, string label, double fte, PoolBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public string Location { get; set; } = location;
    public double Ceiling { get; set; } = ceiling;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public PoolBatchStatus Status { get; set; } = status;

    /// <summary>Starts a pool batch in status planned.</summary>
    public static PoolBatch NewPoolBatch(string code, string name)
    {
        return new PoolBatch(code, name, false, "", 0.0, "", 0.0, PoolBatchStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolBatchStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolBatchStatus next)
    {
        switch (Status)
        {
            case PoolBatchStatus.Planned:
                return next == PoolBatchStatus.InProgress || next == PoolBatchStatus.Cancelled;
            case PoolBatchStatus.Scheduled:
                switch (next)
                {
                    case PoolBatchStatus.InProgress:
                        return true;
                }
                return false;
            case PoolBatchStatus.InProgress:
                switch (next)
                {
                    case PoolBatchStatus.Cancelled:
                        return true;
                }
                return false;
            case PoolBatchStatus.Completed:
                switch (next)
                {
                    case PoolBatchStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pool batch"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Fte * Ceiling, 2);
    }

    public PoolBatch Copy()
    {
        return new PoolBatch(Code, Name, Active, Location, Ceiling, Label, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
