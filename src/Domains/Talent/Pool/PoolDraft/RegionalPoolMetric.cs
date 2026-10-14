namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional pool metric is in its life cycle.</summary>
public enum RegionalPoolMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A regional pool metric as the pool team keeps it.</summary>
public sealed class RegionalPoolMetric(string code, string name, int priority, string employeeId, string ownerId, int startsAt, double threshold, string checksum, string grade, int retries, int batchSize, RegionalPoolMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public string EmployeeId { get; set; } = employeeId;
    public string OwnerId { get; set; } = ownerId;
    public int StartsAt { get; set; } = startsAt;
    public double Threshold { get; set; } = threshold;
    public string Checksum { get; set; } = checksum;
    public string Grade { get; set; } = grade;
    public int Retries { get; set; } = retries;
    public int BatchSize { get; set; } = batchSize;
    public RegionalPoolMetricStatus Status { get; set; } = status;

    /// <summary>Starts a regional pool metric in status idle.</summary>
    public static RegionalPoolMetric NewRegionalPoolMetric(string code, string name)
    {
        return new RegionalPoolMetric(code, name, 0, "", "", 0, 0.0, "", "", 0, 0, RegionalPoolMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPoolMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPoolMetricStatus next)
    {
        switch (Status)
        {
            case RegionalPoolMetricStatus.Idle:
                switch (next)
                {
                    case RegionalPoolMetricStatus.Ready:
                    case RegionalPoolMetricStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalPoolMetricStatus.Warming:
                switch (next)
                {
                    case RegionalPoolMetricStatus.Ready:
                    case RegionalPoolMetricStatus.Draining:
                        return true;
                }
                return false;
            case RegionalPoolMetricStatus.Ready:
                switch (next)
                {
                    case RegionalPoolMetricStatus.Draining:
                    case RegionalPoolMetricStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalPoolMetricStatus.Draining:
                return next == RegionalPoolMetricStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RegionalPoolMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional pool metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)BatchSize - Threshold, 2);
    }

    public RegionalPoolMetric Copy()
    {
        return new RegionalPoolMetric(
            Code,
            Name,
            Priority,
            EmployeeId,
            OwnerId,
            StartsAt,
            Threshold,
            Checksum,
            Grade,
            Retries,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
