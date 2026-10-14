namespace Acme.Workforce.Talent.Pool.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool checkpoint is in its life cycle.</summary>
public enum PoolCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pool checkpoint. Its code is unique within a tenant.</summary>
public sealed class PoolCheckpoint(string code, string name, string location, int updatedAt, int endsAt, bool approved, PoolCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public int UpdatedAt { get; set; } = updatedAt;
    public int EndsAt { get; set; } = endsAt;
    public bool Approved { get; set; } = approved;
    public PoolCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a pool checkpoint in status planned.</summary>
    public static PoolCheckpoint NewPoolCheckpoint(string code, string name)
    {
        return new PoolCheckpoint(code, name, "", 0, 0, false, PoolCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolCheckpointStatus next)
    {
        switch (Status)
        {
            case PoolCheckpointStatus.Planned:
                return next == PoolCheckpointStatus.Cancelled;
            case PoolCheckpointStatus.Scheduled:
                switch (next)
                {
                    case PoolCheckpointStatus.InProgress:
                    case PoolCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
            case PoolCheckpointStatus.InProgress:
                switch (next)
                {
                    case PoolCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
            case PoolCheckpointStatus.Completed:
                return next == PoolCheckpointStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PoolCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)EndsAt - (double)UpdatedAt, 2);
    }

    public PoolCheckpoint Copy()
    {
        return new PoolCheckpoint(Code, Name, Location, UpdatedAt, EndsAt, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
