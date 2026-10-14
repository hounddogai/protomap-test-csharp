namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool tier is in its life cycle.</summary>
public enum PoolTierStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One pool tier, with what the planners need to act on it.</summary>
public sealed class PoolTier(string code, string title, string name, bool archived, string category, string managerId, string notes, int version, int endsAt, double floorValue, int revision, PoolTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string Category { get; set; } = category;
    public string ManagerId { get; set; } = managerId;
    public string Notes { get; set; } = notes;
    public int Version { get; set; } = version;
    public int EndsAt { get; set; } = endsAt;
    public double FloorValue { get; set; } = floorValue;
    public int Revision { get; set; } = revision;
    public PoolTierStatus Status { get; set; } = status;

    /// <summary>Starts a pool tier in status idle.</summary>
    public static PoolTier NewPoolTier(string code, string title)
    {
        return new PoolTier(code, title, "", false, "", "", "", 0, 0, 0.0, 0, PoolTierStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolTierStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolTierStatus next)
    {
        switch (Status)
        {
            case PoolTierStatus.Idle:
                return next == PoolTierStatus.Ready;
            case PoolTierStatus.Warming:
                switch (next)
                {
                    case PoolTierStatus.Draining:
                        return true;
                }
                return false;
            case PoolTierStatus.Ready:
                switch (next)
                {
                    case PoolTierStatus.Draining:
                    case PoolTierStatus.Stopped:
                        return true;
                }
                return false;
            case PoolTierStatus.Draining:
                switch (next)
                {
                    case PoolTierStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetLoad()
    {
        return Math.Round((double)EndsAt + (double)Revision, 2);
    }

    public PoolTier Copy()
    {
        return new PoolTier(
            Code,
            Title,
            Name,
            Archived,
            Category,
            ManagerId,
            Notes,
            Version,
            EndsAt,
            FloorValue,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
