namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback pool metric is in its life cycle.</summary>
public enum FallbackPoolMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A fallback pool metric. Its code is unique within a tenant.</summary>
public sealed class FallbackPoolMetric(string code, string name, string country, string reviewer, int expiresAt, double score, int revision, int batchSize, string region, bool approved, FallbackPoolMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Reviewer { get; set; } = reviewer;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Score { get; set; } = score;
    public int Revision { get; set; } = revision;
    public int BatchSize { get; set; } = batchSize;
    public string Region { get; set; } = region;
    public bool Approved { get; set; } = approved;
    public FallbackPoolMetricStatus Status { get; set; } = status;

    /// <summary>Starts a fallback pool metric in status planned.</summary>
    public static FallbackPoolMetric NewFallbackPoolMetric(string code, string name)
    {
        return new FallbackPoolMetric(code, name, "", "", 0, 0.0, 0, 0, "", false, FallbackPoolMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPoolMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPoolMetricStatus next)
    {
        switch (Status)
        {
            case FallbackPoolMetricStatus.Planned:
                switch (next)
                {
                    case FallbackPoolMetricStatus.InProgress:
                    case FallbackPoolMetricStatus.Completed:
                        return true;
                }
                return false;
            case FallbackPoolMetricStatus.Scheduled:
                switch (next)
                {
                    case FallbackPoolMetricStatus.Completed:
                        return true;
                }
                return false;
            case FallbackPoolMetricStatus.InProgress:
                switch (next)
                {
                    case FallbackPoolMetricStatus.Completed:
                        return true;
                }
                return false;
            case FallbackPoolMetricStatus.Completed:
                return next == FallbackPoolMetricStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FallbackPoolMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback pool metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)ExpiresAt + Score, 2);
    }

    public FallbackPoolMetric Copy()
    {
        return new FallbackPoolMetric(
            Code,
            Name,
            Country,
            Reviewer,
            ExpiresAt,
            Score,
            Revision,
            BatchSize,
            Region,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
