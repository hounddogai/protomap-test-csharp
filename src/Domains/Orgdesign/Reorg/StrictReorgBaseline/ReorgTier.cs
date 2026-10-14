namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg tier is in its life cycle.</summary>
public enum ReorgTierStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A reorg tier as the reorg team keeps it.</summary>
public sealed class ReorgTier(string code, string title, int updatedAt, int createdAt, bool active, int windowDays, int batchSize, int endsAt, string category, string checksum, ReorgTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public int CreatedAt { get; set; } = createdAt;
    public bool Active { get; set; } = active;
    public int WindowDays { get; set; } = windowDays;
    public int BatchSize { get; set; } = batchSize;
    public int EndsAt { get; set; } = endsAt;
    public string Category { get; set; } = category;
    public string Checksum { get; set; } = checksum;
    public ReorgTierStatus Status { get; set; } = status;

    /// <summary>Starts a reorg tier in status idle.</summary>
    public static ReorgTier NewReorgTier(string code, string title)
    {
        return new ReorgTier(code, title, 0, 0, false, 0, 0, 0, "", "", ReorgTierStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgTierStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgTierStatus next)
    {
        switch (Status)
        {
            case ReorgTierStatus.Idle:
                switch (next)
                {
                    case ReorgTierStatus.Draining:
                        return true;
                }
                return false;
            case ReorgTierStatus.Warming:
                return next == ReorgTierStatus.Draining || next == ReorgTierStatus.Stopped;
            case ReorgTierStatus.Ready:
                return next == ReorgTierStatus.Draining || next == ReorgTierStatus.Stopped;
            case ReorgTierStatus.Draining:
                switch (next)
                {
                    case ReorgTierStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetTotal()
    {
        return Math.Round((double)EndsAt + (double)CreatedAt, 2);
    }

    public ReorgTier Copy()
    {
        return new ReorgTier(
            Code,
            Title,
            UpdatedAt,
            CreatedAt,
            Active,
            WindowDays,
            BatchSize,
            EndsAt,
            Category,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
