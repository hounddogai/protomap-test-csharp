namespace Acme.Workforce.Compensation.Benchmark.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered benchmark profile is in its life cycle.</summary>
public enum TieredBenchmarkProfileStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tiered benchmark profile as the benchmark team keeps it.</summary>
public sealed class TieredBenchmarkProfile(string code, string name, int createdAt, int windowDays, string ownerId, string region, TieredBenchmarkProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public int WindowDays { get; set; } = windowDays;
    public string OwnerId { get; set; } = ownerId;
    public string Region { get; set; } = region;
    public TieredBenchmarkProfileStatus Status { get; set; } = status;

    /// <summary>Starts a tiered benchmark profile in status planned.</summary>
    public static TieredBenchmarkProfile NewTieredBenchmarkProfile(string code, string name)
    {
        return new TieredBenchmarkProfile(code, name, 0, 0, "", "", TieredBenchmarkProfileStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredBenchmarkProfileStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredBenchmarkProfileStatus next)
    {
        switch (Status)
        {
            case TieredBenchmarkProfileStatus.Planned:
                return next == TieredBenchmarkProfileStatus.InProgress || next == TieredBenchmarkProfileStatus.Completed;
            case TieredBenchmarkProfileStatus.Scheduled:
                switch (next)
                {
                    case TieredBenchmarkProfileStatus.InProgress:
                        return true;
                }
                return false;
            case TieredBenchmarkProfileStatus.InProgress:
                return next == TieredBenchmarkProfileStatus.Completed || next == TieredBenchmarkProfileStatus.Cancelled;
            case TieredBenchmarkProfileStatus.Completed:
                return next == TieredBenchmarkProfileStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TieredBenchmarkProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered benchmark profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"tiered benchmark profile"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)CreatedAt * (double)WindowDays, 2);
    }

    public TieredBenchmarkProfile Copy()
    {
        return new TieredBenchmarkProfile(Code, Name, CreatedAt, WindowDays, OwnerId, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
