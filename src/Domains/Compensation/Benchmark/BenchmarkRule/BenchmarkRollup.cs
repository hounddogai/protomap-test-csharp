namespace Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a benchmark rollup is in its life cycle.</summary>
public enum BenchmarkRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One benchmark rollup, with what the planners need to act on it.</summary>
public sealed class BenchmarkRollup(string code, string name, string country, string timezone, double ceiling, string segment, int retries, string ownerId, int endsAt, int revision, BenchmarkRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Timezone { get; set; } = timezone;
    public double Ceiling { get; set; } = ceiling;
    public string Segment { get; set; } = segment;
    public int Retries { get; set; } = retries;
    public string OwnerId { get; set; } = ownerId;
    public int EndsAt { get; set; } = endsAt;
    public int Revision { get; set; } = revision;
    public BenchmarkRollupStatus Status { get; set; } = status;

    /// <summary>Starts a benchmark rollup in status planned.</summary>
    public static BenchmarkRollup NewBenchmarkRollup(string code, string name)
    {
        return new BenchmarkRollup(code, name, "", "", 0.0, "", 0, "", 0, 0, BenchmarkRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BenchmarkRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BenchmarkRollupStatus next)
    {
        switch (Status)
        {
            case BenchmarkRollupStatus.Planned:
                return next == BenchmarkRollupStatus.InProgress || next == BenchmarkRollupStatus.Cancelled;
            case BenchmarkRollupStatus.Scheduled:
                return next == BenchmarkRollupStatus.InProgress || next == BenchmarkRollupStatus.Cancelled;
            case BenchmarkRollupStatus.InProgress:
                return next == BenchmarkRollupStatus.Completed || next == BenchmarkRollupStatus.Cancelled;
            case BenchmarkRollupStatus.Completed:
                return next == BenchmarkRollupStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BenchmarkRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"benchmark rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Revision + Ceiling, 2);
    }

    public BenchmarkRollup Copy()
    {
        return new BenchmarkRollup(
            Code,
            Name,
            Country,
            Timezone,
            Ceiling,
            Segment,
            Retries,
            OwnerId,
            EndsAt,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
